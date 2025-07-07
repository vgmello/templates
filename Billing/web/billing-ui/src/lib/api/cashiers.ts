// API functions for cashier management

import { apiClient } from './client.js';
import type {
	Cashier,
	CreateCashierRequest,
	UpdateCashierRequest,
	GetCashiersQuery,
	GetCashiersResult
} from '../types/cashier.js';

export const cashierApi = {
	/**
	 * Get all cashiers with optional filtering
	 */
	async getCashiers(query: GetCashiersQuery = {}): Promise<GetCashiersResult[]> {
		const params = new URLSearchParams();
		
		Object.entries(query).forEach(([key, value]) => {
			if (value !== undefined) {
				params.append(key, value.toString());
			}
		});

		const queryString = params.toString();
		const endpoint = queryString ? `/cashiers?${queryString}` : '/cashiers';
		
		return apiClient.get<GetCashiersResult[]>(endpoint);
	},

	/**
	 * Get a specific cashier by ID
	 */
	async getCashier(id: string): Promise<Cashier> {
		return apiClient.get<Cashier>(`/cashiers/${id}`);
	},

	/**
	 * Create a new cashier
	 */
	async createCashier(data: CreateCashierRequest): Promise<Cashier> {
		return apiClient.post<Cashier>('/cashiers', data);
	},

	/**
	 * Update an existing cashier
	 */
	async updateCashier(id: string, data: UpdateCashierRequest): Promise<Cashier> {
		return apiClient.put<Cashier>(`/cashiers/${id}`, data);
	},

	/**
	 * Delete a cashier
	 */
	async deleteCashier(id: string): Promise<void> {
		return apiClient.delete<void>(`/cashiers/${id}`);
	}
};