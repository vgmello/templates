// Server-side API client for direct backend communication
// This bypasses the Vite proxy and talks directly to the backend

import { context, trace } from '@opentelemetry/api';

export class ServerApiError extends Error {
	constructor(
		message: string,
		public status: number,
		public data?: any
	) {
		super(message);
		this.name = 'ServerApiError';
	}
}

class ServerApiClient {
	private baseUrl: string;
	private tracer = trace.getTracer('billing-ui-api-client', '1.0.0');

	constructor(baseUrl = 'http://localhost:8101') {
		this.baseUrl = baseUrl;
	}

	private async request<T>(
		endpoint: string,
		options: RequestInit = {},
		traceContext?: App.Locals['traceContext']
	): Promise<T> {
		const url = `${this.baseUrl}${endpoint}`;
		const spanName = `${options.method || 'GET'} ${endpoint}`;

		return this.tracer.startActiveSpan(spanName, async (span) => {
			try {
				// Prepare headers (trace context will be automatically injected by fetch instrumentation)
				const headers = {
					'Content-Type': 'application/json',
					...options.headers,
				};

				const config: RequestInit = {
					...options,
					headers
				};

				// Add span attributes
				span.setAttributes({
					'http.method': options.method || 'GET',
					'http.url': url,
					'http.target': endpoint,
					'peer.service': 'billing-api'
				});

				const response = await fetch(url, config);

				// Add response attributes
				span.setAttributes({
					'http.status_code': response.status,
					'http.response.content_length': response.headers.get('content-length') || '0'
				});

				if (!response.ok) {
					let errorData;
					try {
						errorData = await response.json();
					} catch {
						errorData = { message: response.statusText };
					}

					const error = new ServerApiError(
						errorData.message || `HTTP ${response.status}`,
						response.status,
						errorData
					);

					span.recordException(error);
					span.setStatus({ code: 2, message: error.message });
					throw error;
				}

				// Handle 204 No Content responses
				if (response.status === 204) {
					span.setStatus({ code: 1 }); // OK
					return {} as T;
				}

				const data = await response.json();
				span.setStatus({ code: 1 }); // OK
				return data;
			} catch (error) {
				if (!(error instanceof ServerApiError)) {
					const networkError = new ServerApiError('Network error', 0, error);
					span.recordException(networkError);
					span.setStatus({ code: 2, message: 'Network error' });
					throw networkError;
				}
				throw error;
			} finally {
				span.end();
			}
		});
	}

	async get<T>(endpoint: string, traceContext?: App.Locals['traceContext']): Promise<T> {
		return this.request<T>(endpoint, { method: 'GET' }, traceContext);
	}

	async post<T>(endpoint: string, data?: any, traceContext?: App.Locals['traceContext']): Promise<T> {
		return this.request<T>(endpoint, {
			method: 'POST',
			body: data ? JSON.stringify(data) : undefined,
		}, traceContext);
	}

	async put<T>(endpoint: string, data?: any, traceContext?: App.Locals['traceContext']): Promise<T> {
		return this.request<T>(endpoint, {
			method: 'PUT',
			body: data ? JSON.stringify(data) : undefined,
		}, traceContext);
	}

	async delete<T>(endpoint: string, traceContext?: App.Locals['traceContext']): Promise<T> {
		return this.request<T>(endpoint, { method: 'DELETE' }, traceContext);
	}
}

export const serverApiClient = new ServerApiClient();