// API client configuration and base functions

import { getTracer, createSpan } from '$lib/telemetry/tracing.js';
import { BROWSER } from 'esm-env';

export class ApiError extends Error {
	constructor(
		message: string,
		public status: number,
		public data?: any
	) {
		super(message);
		this.name = 'ApiError';
	}
}

class ApiClient {
	private baseUrl: string;
	private tracer = BROWSER ? getTracer('billing-ui-client') : null;

	constructor(baseUrl = '/api') {
		this.baseUrl = baseUrl;
	}

	private async request<T>(
		endpoint: string,
		options: RequestInit = {}
	): Promise<T> {
		const url = `${this.baseUrl}${endpoint}`;
		const method = options.method || 'GET';
		const spanName = `${method} ${endpoint}`;

		// If tracing is available, create a span for this request
		if (this.tracer && BROWSER) {
			return this.tracer.startActiveSpan(spanName, async (span) => {
				try {
					span.setAttributes({
						'http.method': method,
						'http.url': url,
						'http.target': endpoint,
						'component': 'billing-ui-client'
					});

					const result = await this.executeRequest<T>(url, options);
					span.setStatus({ code: 1 }); // OK
					return result;
				} catch (error) {
					span.recordException(error as Error);
					span.setStatus({ 
						code: 2, 
						message: error instanceof Error ? error.message : 'Unknown error' 
					});
					throw error;
				} finally {
					span.end();
				}
			});
		} else {
			// Fallback for server-side rendering or when tracing is not available
			return this.executeRequest<T>(url, options);
		}
	}

	private async executeRequest<T>(url: string, options: RequestInit): Promise<T> {
		const config: RequestInit = {
			headers: {
				'Content-Type': 'application/json',
				...options.headers,
			},
			...options,
		};

		try {
			const response = await fetch(url, config);
			
			if (!response.ok) {
				let errorData;
				try {
					errorData = await response.json();
				} catch {
					errorData = { message: response.statusText };
				}
				
				throw new ApiError(
					errorData.message || `HTTP ${response.status}`,
					response.status,
					errorData
				);
			}

			// Handle 204 No Content responses
			if (response.status === 204) {
				return {} as T;
			}

			return await response.json();
		} catch (error) {
			if (error instanceof ApiError) {
				throw error;
			}
			throw new ApiError('Network error', 0, error);
		}
	}

	async get<T>(endpoint: string): Promise<T> {
		return this.request<T>(endpoint, { method: 'GET' });
	}

	async post<T>(endpoint: string, data?: any): Promise<T> {
		return this.request<T>(endpoint, {
			method: 'POST',
			body: data ? JSON.stringify(data) : undefined,
		});
	}

	async put<T>(endpoint: string, data?: any): Promise<T> {
		return this.request<T>(endpoint, {
			method: 'PUT',
			body: data ? JSON.stringify(data) : undefined,
		});
	}

	async delete<T>(endpoint: string): Promise<T> {
		return this.request<T>(endpoint, { method: 'DELETE' });
	}
}

export const apiClient = new ApiClient();