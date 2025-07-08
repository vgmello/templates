import { trace } from '@opentelemetry/api';
import { config } from '../config/env';

export class ApiError extends Error {
	constructor(
		message: string,
		public readonly status: number,
		public readonly data?: any,
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
	body?: any;
	params?: Record<string, string | number | boolean>;
	timeout?: number;
}

export class ApiClient {
	private readonly baseUrl: string;
	private readonly tracer = trace.getTracer('billing-ui-api-client', '1.0.0');

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

	private async request<T>(
		endpoint: string,
		options: ApiRequestOptions = {}
	): Promise<T> {
		const method = options.method || 'GET';
		const url = this.buildUrl(endpoint, options.params);
		const spanName = `${method} ${endpoint}`;

		return this.tracer.startActiveSpan(spanName, async (span) => {
			try {
				const headers: Record<string, string> = {
					'Content-Type': 'application/json',
					...options.headers
				};

				const requestConfig: RequestInit = {
					method,
					headers,
					body: options.body ? JSON.stringify(options.body) : undefined
				};

				// Add span attributes
				span.setAttributes({
					'http.method': method,
					'http.url': url,
					'http.target': endpoint,
					'peer.service': 'billing-api',
					'user_agent.original': typeof navigator !== 'undefined' ? navigator.userAgent : 'server'
				});

				const controller = new AbortController();
				const timeoutId = setTimeout(() => controller.abort(), options.timeout || 30000);

				const response = await fetch(url, {
					...requestConfig,
					signal: controller.signal
				});

				clearTimeout(timeoutId);

				// Add response attributes
				span.setAttributes({
					'http.status_code': response.status,
					'http.response.content_length': response.headers.get('content-length') || '0'
				});

				if (!response.ok) {
					let errorData: any;
					try {
						errorData = await response.json();
					} catch {
						errorData = { message: response.statusText };
					}

					const error = new ApiError(
						errorData.message || `HTTP ${response.status}: ${response.statusText}`,
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
				if (error instanceof ApiError) {
					throw error;
				}

				// Handle network errors or timeout
				const networkError = new ApiError(
					'Network error or timeout',
					0,
					error,
					endpoint
				);

				span.recordException(networkError);
				span.setStatus({ code: 2, message: 'Network error' });
				throw networkError;
			} finally {
				span.end();
			}
		});
	}

	async get<T>(endpoint: string, params?: Record<string, string | number | boolean>): Promise<T> {
		return this.request<T>(endpoint, { method: 'GET', params });
	}

	async post<T>(endpoint: string, body?: any): Promise<T> {
		return this.request<T>(endpoint, { method: 'POST', body });
	}

	async put<T>(endpoint: string, body?: any): Promise<T> {
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
				await new Promise(resolve => setTimeout(resolve, Math.pow(2, i) * 1000));
			}
		}
		throw new Error('Max retries exceeded');
	}
}

// Create singleton instances
export const apiClient = new ApiClient();