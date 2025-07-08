import { BROWSER } from 'esm-env';
import { trace } from '@opentelemetry/api';
import { config } from '../config/env';

export class ApiError extends Error {
	constructor(
		message: string,
		public readonly status: number,
		public readonly data?: unknown,
		public readonly endpoint?: string
	) {
		super(message);
		this.name = 'ApiError';
	}

	static isNetworkError(error: unknown): boolean {
		return error instanceof ApiError && error.status === 0;
	}

	static isServerError(error: unknown): boolean {
		return error instanceof ApiError && error.status >= 500;
	}

	static isClientError(error: unknown): boolean {
		return error instanceof ApiError && error.status >= 400 && error.status < 500;
	}
}

export interface ApiRequestOptions {
	method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
	headers?: Record<string, string>;
	body?: unknown;
	params?: Record<string, string | number | boolean>;
	timeout?: number;
}

// Input sanitization utilities
class InputSanitizer {
	private static readonly MAX_STRING_LENGTH = 10000;
	private static readonly MAX_OBJECT_DEPTH = 10;
	private static readonly DANGEROUS_PATTERNS = [
		/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
		/javascript:/gi,
		/data:text\/html/gi,
		/on\w+\s*=/gi
	];

	static sanitizeValue(value: unknown, depth = 0): unknown {
		if (depth > this.MAX_OBJECT_DEPTH) {
			throw new Error('Object depth limit exceeded');
		}

		if (value === null || value === undefined) {
			return value;
		}

		if (typeof value === 'string') {
			return this.sanitizeString(value);
		}

		if (typeof value === 'number' || typeof value === 'boolean') {
			return value;
		}

		if (value instanceof Date) {
			return value.toISOString();
		}

		if (Array.isArray(value)) {
			if (value.length > 1000) {
				throw new Error('Array length limit exceeded');
			}
			return value.map(item => this.sanitizeValue(item, depth + 1));
		}

		if (typeof value === 'object') {
			const keys = Object.keys(value);
			if (keys.length > 100) {
				throw new Error('Object key limit exceeded');
			}

			const sanitized: Record<string, unknown> = {};
			for (const key of keys) {
				const sanitizedKey = this.sanitizeString(key);
				sanitized[sanitizedKey] = this.sanitizeValue((value as Record<string, unknown>)[key], depth + 1);
			}
			return sanitized;
		}

		throw new Error(`Unsupported value type: ${typeof value}`);
	}

	private static sanitizeString(str: string): string {
		if (str.length > this.MAX_STRING_LENGTH) {
			throw new Error(`String length exceeds limit of ${this.MAX_STRING_LENGTH} characters`);
		}

		// Check for dangerous patterns
		for (const pattern of this.DANGEROUS_PATTERNS) {
			if (pattern.test(str)) {
				throw new Error('Input contains potentially dangerous content');
			}
		}

		// Basic HTML encoding for safety
		return str
			.replace(/&/g, '&amp;')
			.replace(/</g, '&lt;')
			.replace(/>/g, '&gt;')
			.replace(/"/g, '&quot;')
			.replace(/'/g, '&#x27;')
			.trim();
	}
}

export class ApiClient {
	private readonly baseUrl: string;
	private readonly tracer = trace.getTracer('billing-ui-api-client', '1.0.0');
	
	/**
	 * TODO: Bundle Size Optimization
	 * The current implementation includes OpenTelemetry dependencies in the client bundle.
	 * For production optimization, consider:
	 * 1. Moving telemetry to server-side only (hooks.server.ts handles this)
	 * 2. Using conditional imports to exclude telemetry from client build
	 * 3. Implementing lightweight client-side logging instead of full OTel
	 * 
	 * Current telemetry packages add ~200KB to bundle:
	 * - @opentelemetry/api, @opentelemetry/sdk-trace-web, etc.
	 */

	constructor(baseUrl?: string) {
		this.baseUrl = baseUrl || config.apiBaseUrl;
	}

	private buildUrl(endpoint: string, params?: Record<string, string | number | boolean>): string {
		const url = new URL(endpoint, this.baseUrl);

		if (params) {
			Object.entries(params).forEach(([key, value]) => {
				if (value !== undefined && value !== null) {
					url.searchParams.append(key, String(value));
				}
			});
		}

		return url.toString();
	}

	private async request<T>(endpoint: string, options: ApiRequestOptions = {}): Promise<T> {
		const method = options.method || 'GET';
		const url = this.buildUrl(endpoint, options.params);
		const spanName = `${method} ${endpoint}`;

		return this.tracer.startActiveSpan(spanName, async (span) => {
			const controller = new AbortController();
			let timeoutId: ReturnType<typeof setTimeout> | null = null;
			let isCompleted = false;

			try {
				const headers: Record<string, string> = {
					'Content-Type': 'application/json',
					...options.headers
				};

				// Sanitize request body if present
				let sanitizedBody: string | undefined;
				if (options.body) {
					try {
						const sanitized = InputSanitizer.sanitizeValue(options.body);
						sanitizedBody = JSON.stringify(sanitized);
					} catch (error) {
						const sanitizeError = new ApiError(
							`Input validation failed: ${error instanceof Error ? error.message : 'Invalid input'}`,
							400,
							error,
							endpoint
						);
						span.recordException(sanitizeError);
						span.setStatus({ code: 2, message: sanitizeError.message });
						throw sanitizeError;
					}
				}

				const requestConfig: RequestInit = {
					method,
					headers,
					body: sanitizedBody
				};

				// Add span attributes
				span.setAttributes({
					'http.method': method,
					'http.url': url,
					'http.target': endpoint,
					'peer.service': 'billing-api',
					'user_agent.original':
						typeof navigator !== 'undefined' ? navigator.userAgent : 'server'
				});

				// Set up timeout that checks if request is still active
				timeoutId = setTimeout(() => {
					if (!isCompleted && !controller.signal.aborted) {
						controller.abort();
					}
				}, options.timeout || 30000);

				const response = await fetch(url, {
					...requestConfig,
					signal: controller.signal
				});

				// Mark as completed and clear timeout immediately
				isCompleted = true;
				if (timeoutId !== null) {
					clearTimeout(timeoutId);
					timeoutId = null;
				}

				// Add response attributes
				span.setAttributes({
					'http.status_code': response.status,
					'http.response.content_length': response.headers.get('content-length') || '0'
				});

				if (!response.ok) {
					let errorData: unknown;
					try {
						errorData = await response.json();
					} catch {
						errorData = { message: response.statusText };
					}

					const error = new ApiError(
						(errorData as { message?: string }).message ||
							`HTTP ${response.status}: ${response.statusText}`,
						response.status,
						errorData,
						endpoint
					);

					span.recordException(error);
					span.setStatus({ code: 2, message: error.message });
					throw error;
				}

				// Handle 204 No Content responses
				if (response.status === 204) {
					span.setStatus({ code: 1 });
					return {} as T;
				}

				const data = await response.json();
				span.setStatus({ code: 1 });
				return data;
			} catch (error) {
				// Mark as completed and ensure timeout is cleared
				isCompleted = true;
				if (timeoutId !== null) {
					clearTimeout(timeoutId);
					timeoutId = null;
				}

				// Handle abort errors specially (timeout case)
				if (error instanceof Error && error.name === 'AbortError') {
					const timeoutError = new ApiError('Request timeout', 408, error, endpoint);
					span.recordException(timeoutError);
					span.setStatus({ code: 2, message: 'Request timeout' });
					throw timeoutError;
				}

				// Re-throw ApiError as-is
				if (error instanceof ApiError) {
					throw error;
				}

				// Handle other network errors
				const networkError = new ApiError(
					'Network error or connection failed',
					0,
					error,
					endpoint
				);

				span.recordException(networkError);
				span.setStatus({ code: 2, message: 'Network error' });
				throw networkError;
			} finally {
				// Final cleanup - ensure timeout is always cleared
				if (timeoutId !== null) {
					clearTimeout(timeoutId);
				}
				span.end();
			}
		});
	}

	async get<T>(endpoint: string, params?: Record<string, string | number | boolean>): Promise<T> {
		return this.request<T>(endpoint, { method: 'GET', params });
	}

	async post<T>(endpoint: string, body?: unknown): Promise<T> {
		return this.request<T>(endpoint, { method: 'POST', body });
	}

	async put<T>(endpoint: string, body?: unknown): Promise<T> {
		return this.request<T>(endpoint, { method: 'PUT', body });
	}

	async delete<T>(endpoint: string): Promise<T> {
		return this.request<T>(endpoint, { method: 'DELETE' });
	}

	// Helper methods for common patterns
	async getWithRetry<T>(
		endpoint: string,
		params?: Record<string, string | number | boolean>,
		retries = 3
	): Promise<T> {
		for (let i = 0; i <= retries; i++) {
			try {
				return await this.get<T>(endpoint, params);
			} catch (error) {
				if (i === retries || !ApiError.isNetworkError(error)) {
					throw error;
				}
				// Wait before retry: 1s, 2s, 4s
				await new Promise((resolve) => setTimeout(resolve, Math.pow(2, i) * 1000));
			}
		}
		throw new Error('Max retries exceeded');
	}
}

// Create singleton instances
export const apiClient = new ApiClient();
