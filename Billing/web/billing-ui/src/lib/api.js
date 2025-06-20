const API_BASE_URL = 'http://localhost:5061';

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
		const response = await fetch(url, config);
		
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
	async getCashiers() {
		try {
			return await apiRequest('/cashiers');
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

	async getCashier(id) {
		try {
			return await apiRequest(`/cashiers/${id}`);
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

	async createCashier(cashierData) {
		return apiRequest('/cashiers', {
			method: 'POST',
			body: JSON.stringify(cashierData)
		});
	},

	async updateCashier(id, cashierData) {
		return apiRequest(`/cashiers/${id}`, {
			method: 'PUT',
			body: JSON.stringify(cashierData)
		});
	},

	async deleteCashier(id) {
		return apiRequest(`/cashiers/${id}`, {
			method: 'DELETE'
		});
	}
};