// Use the proxy configured in vite.config.js
const API_BASE_URL = '/api';

class ApiError extends Error {
	constructor(message, status) {
		super(message);
		this.status = status;
	}
}

async function apiRequest(endpoint, options = {}) {
	const url = `${API_BASE_URL}${endpoint}`;
	
	const config = {
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
		
		return await response.text();
	} catch (error) {
		if (error instanceof ApiError) {
			throw error;
		}
		throw new ApiError(`Network error: ${error.message}`, 0);
	}
}

export const cashierService = {
	async getCashiers(fetchFn) {
		try {
			return await apiRequest('/cashiers', { fetch: fetchFn });
		} catch (error) {
			// If API fails, return mock data for testing
			console.warn('API failed, using mock data:', error.message);
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

	async getCashier(id, fetchFn) {
		try {
			return await apiRequest(`/cashiers/${id}`, { fetch: fetchFn });
		} catch (error) {
			// If API fails, return mock data for testing
			console.warn('API failed, using mock data:', error.message);
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

	async createCashier(cashierData, fetchFn) {
		return apiRequest('/cashiers', {
			method: 'POST',
			body: JSON.stringify(cashierData),
			fetch: fetchFn
		});
	},

	async updateCashier(id, cashierData, fetchFn) {
		return apiRequest(`/cashiers/${id}`, {
			method: 'PUT',
			body: JSON.stringify(cashierData),
			fetch: fetchFn
		});
	},

	async deleteCashier(id, fetchFn) {
		return apiRequest(`/cashiers/${id}`, {
			method: 'DELETE',
			fetch: fetchFn
		});
	}
};