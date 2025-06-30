// Invoice API client

import { apiClient } from './client.js';
import type { 
	Invoice, 
	CreateInvoiceRequest, 
	MarkInvoiceAsPaidRequest, 
	SimulatePaymentRequest, 
	GetInvoicesQuery,
	InvoiceSummary 
} from '$lib/types/invoice.js';

export class InvoiceApi {
	/**
	 * Get all invoices with optional filtering
	 */
	async getInvoices(query?: GetInvoicesQuery): Promise<Invoice[]> {
		const params = new URLSearchParams();
		
		if (query?.status) params.append('status', query.status);
		if (query?.cashierId) params.append('cashierId', query.cashierId);
		if (query?.fromDate) params.append('fromDate', query.fromDate);
		if (query?.toDate) params.append('toDate', query.toDate);
		if (query?.skip !== undefined) params.append('skip', query.skip.toString());
		if (query?.take !== undefined) params.append('take', query.take.toString());

		const queryString = params.toString();
		const endpoint = queryString ? `/invoices?${queryString}` : '/invoices';
		
		return apiClient.get<Invoice[]>(endpoint);
	}

	/**
	 * Get a specific invoice by ID
	 */
	async getInvoice(id: string): Promise<Invoice> {
		return apiClient.get<Invoice>(`/invoices/${id}`);
	}

	/**
	 * Create a new invoice
	 */
	async createInvoice(invoice: CreateInvoiceRequest): Promise<Invoice> {
		return apiClient.post<Invoice>('/invoices', invoice);
	}

	/**
	 * Cancel an invoice
	 */
	async cancelInvoice(id: string): Promise<Invoice> {
		return apiClient.put<Invoice>(`/invoices/${id}/cancel`);
	}

	/**
	 * Mark an invoice as paid
	 */
	async markInvoiceAsPaid(id: string, request: MarkInvoiceAsPaidRequest): Promise<Invoice> {
		return apiClient.put<Invoice>(`/invoices/${id}/mark-paid`, request);
	}

	/**
	 * Simulate a payment for testing
	 */
	async simulatePayment(id: string, request: SimulatePaymentRequest): Promise<void> {
		return apiClient.post<void>(`/invoices/${id}/simulate-payment`, request);
	}

	/**
	 * Calculate invoice summary statistics
	 */
	async getInvoiceSummary(): Promise<InvoiceSummary> {
		const invoices = await this.getInvoices();
		
		return {
			totalInvoices: invoices.length,
			totalAmount: invoices.reduce((sum, invoice) => sum + invoice.amount, 0),
			paidCount: invoices.filter(invoice => invoice.status === 'Paid').length,
			overdueCount: invoices.filter(invoice => {
				if (invoice.status === 'Paid' || invoice.status === 'Cancelled') return false;
				if (!invoice.dueDate) return false;
				return new Date(invoice.dueDate) < new Date();
			}).length
		};
	}
}

export const invoiceApi = new InvoiceApi();