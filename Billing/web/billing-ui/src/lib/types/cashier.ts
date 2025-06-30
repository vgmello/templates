// TypeScript interfaces matching the backend Cashier models

export interface CashierPayment {
	// This would be populated later when we have payment data
	// Based on the backend models, this is referenced but not detailed
}

export interface Cashier {
	cashierId: string;
	tenantId: string;
	name: string;
	email: string;
	cashierPayments: CashierPayment[];
}

export interface CreateCashierRequest {
	name: string;
	email: string;
}

export interface UpdateCashierRequest {
	cashierId?: string; // Will be set by the API call
	name: string;
	email: string;
}

export interface GetCashiersQuery {
	// Based on the controller, this supports query parameters for filtering
	// Adding common pagination and search parameters
	page?: number;
	pageSize?: number;
	search?: string;
	sortBy?: string;
	sortOrder?: 'asc' | 'desc';
}

export interface GetCashiersResult {
	cashierId: string;
	name: string;
	email: string;
	// Based on the controller response type GetCashiersQuery.Result
	// Adding created date for display purposes
	createdDateUtc?: string;
}

export interface ApiErrorResponse {
	errors: string[];
}