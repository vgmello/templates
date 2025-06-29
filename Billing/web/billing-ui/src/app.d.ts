// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		// interface Error {}
		// interface Locals {}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
	}
}

// Cashier types
export interface CashierPayment {
	currency: string;
	isActive: boolean;
	createdDateUtc: string;
	description?: string;
}

export interface Cashier {
	tenantId: string;
	cashierId: string;
	name: string;
	email: string;
	cashierPayments: CashierPayment[];
	createdDateUtc: string;
	updatedDateUtc: string;
	version: number;
}

export interface CreateCashierRequest {
	name: string;
	email: string;
	currencies: string[];
}

export interface UpdateCashierRequest {
	name?: string;
	email?: string;
	currencies?: string[];
}

// Invoice types
export interface Invoice {
	invoiceId: string;
	name: string;
	status: string;
	amount: number;
	currency: string;
	dueDate?: string;
	cashierId?: string;
	createdDateUtc: string;
	updatedDateUtc: string;
	version: number;
}

export interface CreateInvoiceRequest {
	name: string;
	amount: number;
	currency?: string;
	dueDate?: string;
	cashierId?: string;
}

export interface UpdateInvoiceRequest {
	name?: string;
	amount?: number;
	currency?: string;
	dueDate?: string;
	cashierId?: string;
}

export interface MarkInvoiceAsPaidRequest {
	amountPaid: number;
	paymentDate?: string;
}

export interface SimulatePaymentRequest {
	amount: number;
	currency?: string;
	paymentMethod?: string;
	paymentReference?: string;
}

export {};