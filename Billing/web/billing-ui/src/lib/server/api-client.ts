// Server-side API client for direct backend communication
// This bypasses the Vite proxy and talks directly to the backend

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

	constructor(baseUrl = 'http://localhost:8101') {
		this.baseUrl = baseUrl;
	}

	private async request<T>(
		endpoint: string,
		options: RequestInit = {}
	): Promise<T> {
		const url = `${this.baseUrl}${endpoint}`;
		
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
				
				throw new ServerApiError(
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
			if (error instanceof ServerApiError) {
				throw error;
			}
			throw new ServerApiError('Network error', 0, error);
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

export const serverApiClient = new ServerApiClient();