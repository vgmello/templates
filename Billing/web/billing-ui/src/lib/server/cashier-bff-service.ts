// Cashier BFF (Backend for Frontend) service
// Handles cashier-related data aggregation and optimization

import { serverApiClient } from './api-client.js';
import type { 
	Cashier,
	GetCashiersResult,
	GetCashiersQuery
} from '$lib/types/cashier.js';

export class CashierBffService {
	// Cashiers list
	async getCashiers(query: GetCashiersQuery = {}): Promise<GetCashiersResult[]> {
		const params = new URLSearchParams();
		
		Object.entries(query).forEach(([key, value]) => {
			if (value !== undefined) {
				params.append(key, value.toString());
			}
		});

		const queryString = params.toString();
		const endpoint = queryString ? `/cashiers?${queryString}` : '/cashiers';
		
		return serverApiClient.get<GetCashiersResult[]>(endpoint);
	}

	// Single cashier data
	async getCashier(id: string): Promise<Cashier> {
		return serverApiClient.get<Cashier>(`/cashiers/${id}`);
	}
}

export const cashierBffService = new CashierBffService();