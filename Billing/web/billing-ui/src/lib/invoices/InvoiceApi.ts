import { apiClient } from '$lib/infrastructure/api/ApiClient';

export interface Invoice {
	invoiceId: string;
	name: string;
	status: string;
	amount: number;
	currency?: string;
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

export interface GetInvoicesQuery {
	status?: string;
	cashierId?: string;
	fromDate?: string;
	toDate?: string;
	skip?: number;
	take?: number;
	[key: string]: string | number | boolean | undefined;
}

export const invoiceApi = {
	async getInvoices(query?: GetInvoicesQuery): Promise<Invoice[]> {
		return apiClient.get<Invoice[]>('/invoices', query);
	},

	async getInvoice(id: string): Promise<Invoice> {
		return apiClient.get<Invoice>(`/invoices/${id}`);
	},

	async createInvoice(request: CreateInvoiceRequest): Promise<Invoice> {
		return apiClient.post<Invoice>('/invoices', request);
	},

	async updateInvoice(id: string, request: Partial<CreateInvoiceRequest>): Promise<Invoice> {
		return apiClient.put<Invoice>(`/invoices/${id}`, request);
	},

	async cancelInvoice(id: string): Promise<void> {
		return apiClient.put<void>(`/invoices/${id}/cancel`);
	},

	async markInvoiceAsPaid(id: string, request: MarkInvoiceAsPaidRequest): Promise<void> {
		return apiClient.put<void>(`/invoices/${id}/mark-paid`, request);
	},

	async simulatePayment(id: string, request: SimulatePaymentRequest): Promise<void> {
		return apiClient.post<void>(`/invoices/${id}/simulate-payment`, request);
	}
};
