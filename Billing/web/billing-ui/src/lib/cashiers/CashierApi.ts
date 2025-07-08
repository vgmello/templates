import { apiClient } from '$lib/infrastructure/api/ApiClient';

export interface GetCashiersResult {
	cashierId: string;
	name: string;
	email: string;
	createdDateUtc?: string;
}

export interface Cashier {
	cashierId: string;
	tenantId: string;
	name: string;
	email: string;
	cashierPayments: unknown[];
}

export interface CreateCashierRequest {
	name: string;
	email: string;
}

export interface UpdateCashierRequest {
	cashierId?: string;
	name: string;
	email: string;
}

export interface GetCashiersQuery {
	page?: number;
	pageSize?: number;
	search?: string;
	sortBy?: string;
	sortOrder?: 'asc' | 'desc';
	[key: string]: string | number | boolean | undefined;
}

export const cashierApi = {
	async getCashiers(query?: GetCashiersQuery): Promise<GetCashiersResult[]> {
		return apiClient.get<GetCashiersResult[]>('/cashiers', query);
	},

	async getCashier(id: string): Promise<Cashier> {
		return apiClient.get<Cashier>(`/cashiers/${id}`);
	},

	async createCashier(request: CreateCashierRequest): Promise<Cashier> {
		return apiClient.post<Cashier>('/cashiers', request);
	},

	async updateCashier(id: string, request: UpdateCashierRequest): Promise<Cashier> {
		return apiClient.put<Cashier>(`/cashiers/${id}`, request);
	},

	async deleteCashier(id: string): Promise<void> {
		return apiClient.delete<void>(`/cashiers/${id}`);
	}
};
