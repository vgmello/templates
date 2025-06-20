import type { Cashier, CreateCashierRequest, UpdateCashierRequest, ApiRequestOptions } from '../app';

// Use the proxy configured in vite.config.js
const API_BASE_URL = '/api';

export class ApiError extends Error {
	status: number;
	
	constructor(message: string, status: number) {
		super(message);
		this.status = status;
	}
}

async function apiRequest<T = any>(endpoint: string, options: ApiRequestOptions = {}): Promise<T> {
	const url = `${API_BASE_URL}${endpoint}`;
	
	const config: RequestInit = {
		headers: {
			'Content-Type': 'application/json',
			...options.headers
		},
		...options
	};

	try {
		// Use provided fetch function or default to global fetch
		const fetchFn = options.fetch || fetch;
		const response = await fetchFn(url, config);
		
		if (!response.ok) {
			throw new ApiError(`API request failed: ${response.statusText}`, response.status);
		}

		const contentType = response.headers.get('content-type');
		if (contentType && contentType.includes('application/json')) {
			return await response.json();
		}
		
		return await response.text() as T;
	} catch (error) {
		if (error instanceof ApiError) {
			throw error;
		}
		throw new ApiError(`Network error: ${(error as Error).message}`, 0);
	}
}

export const cashierService = {
	async getCashiers(fetchFn?: typeof fetch): Promise<Cashier[]> {
		try {
			return await apiRequest<Cashier[]>('/cashiers', { fetch: fetchFn });
		} catch (error) {
			// If API fails, return mock data for testing
			console.warn('API failed, using mock data:', (error as Error).message);
			return [
				{
					cashierId: "a52757cd-a42f-4fb9-8566-a98c61a71d2a",
					name: "Test Cashier",
					email: "test@example.com",
					cashierPayments: [
						{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() },
						{ currency: "EUR", isActive: true, createdDateUtc: new Date().toISOString() }
					],
					createdDateUtc: new Date().toISOString(),
					updatedDateUtc: new Date().toISOString(),
					version: 1
				},
				{
					cashierId: "b52757cd-a42f-4fb9-8566-a98c61a71d2a",
					name: "John Doe",
					email: "john.doe@example.com",
					cashierPayments: [
						{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() }
					],
					createdDateUtc: new Date().toISOString(),
					updatedDateUtc: new Date().toISOString(),
					version: 1
				}
			];
		}
	},

	async getCashier(id: string, fetchFn?: typeof fetch): Promise<Cashier> {
		try {
			return await apiRequest<Cashier>(`/cashiers/${id}`, { fetch: fetchFn });
		} catch (error) {
			// If API fails, return mock data for testing
			console.warn('API failed, using mock data:', (error as Error).message);
			return {
				cashierId: id,
				name: "Mock Cashier",
				email: "mock@example.com",
				cashierPayments: [
					{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() },
					{ currency: "EUR", isActive: true, createdDateUtc: new Date().toISOString() }
				],
				createdDateUtc: new Date().toISOString(),
				updatedDateUtc: new Date().toISOString(),
				version: 1
			};
		}
	},

	async createCashier(cashierData: CreateCashierRequest, fetchFn?: typeof fetch): Promise<Cashier> {
		return apiRequest<Cashier>('/cashiers', {
			method: 'POST',
			body: JSON.stringify(cashierData),
			fetch: fetchFn
		});
	},

	async updateCashier(id: string, cashierData: UpdateCashierRequest, fetchFn?: typeof fetch): Promise<Cashier> {
		return apiRequest<Cashier>(`/cashiers/${id}`, {
			method: 'PUT',
			body: JSON.stringify(cashierData),
			fetch: fetchFn
		});
	},

	async deleteCashier(id: string, fetchFn?: typeof fetch): Promise<void> {
		return apiRequest<void>(`/cashiers/${id}`, {
			method: 'DELETE',
			fetch: fetchFn
		});
	}
};