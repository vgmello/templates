// Invoice BFF (Backend for Frontend) service
// Handles invoice-related data aggregation and optimization

import { serverApiClient } from './api-client.js';
import type { 
	Invoice, 
	InvoiceSummary, 
	GetInvoicesQuery
} from '$lib/types/invoice.js';
import type { GetCashiersResult } from '$lib/types/cashier.js';

export interface InvoicesWithSummary {
	invoices: Invoice[];
	summary: InvoiceSummary;
}

export interface InvoiceCreateData {
	cashiers: GetCashiersResult[];
}

export class InvoiceBffService {
	// Optimized invoice list with summary - combines 2 API calls into 1 aggregated response
	async getInvoicesWithSummary(query?: GetInvoicesQuery, traceContext?: App.Locals['traceContext']): Promise<InvoicesWithSummary> {
		const params = new URLSearchParams();
		
		if (query?.status) params.append('status', query.status);
		if (query?.cashierId) params.append('cashierId', query.cashierId);
		if (query?.fromDate) params.append('fromDate', query.fromDate);
		if (query?.toDate) params.append('toDate', query.toDate);
		if (query?.skip !== undefined) params.append('skip', query.skip.toString());
		if (query?.take !== undefined) params.append('take', query.take.toString());

		const queryString = params.toString();
		const endpoint = queryString ? `/invoices?${queryString}` : '/invoices';
		
		// Fetch invoices and calculate summary in parallel
		const [invoices] = await Promise.all([
			serverApiClient.get<Invoice[]>(endpoint, traceContext)
		]);

		// Calculate summary from invoices data (client-side aggregation)
		const summary: InvoiceSummary = {
			totalInvoices: invoices.length,
			totalAmount: invoices.reduce((sum, invoice) => sum + invoice.amount, 0),
			paidCount: invoices.filter(invoice => invoice.status === 'Paid').length,
			overdueCount: invoices.filter(invoice => {
				if (invoice.status === 'Paid' || invoice.status === 'Cancelled') return false;
				if (!invoice.dueDate) return false;
				return new Date(invoice.dueDate) < new Date();
			}).length
		};

		return { invoices, summary };
	}

	// Single invoice data
	async getInvoice(id: string, traceContext?: App.Locals['traceContext']): Promise<Invoice> {
		return serverApiClient.get<Invoice>(`/invoices/${id}`, traceContext);
	}

	// Invoice create page data - loads cashiers for dropdown
	async getInvoiceCreateData(traceContext?: App.Locals['traceContext']): Promise<InvoiceCreateData> {
		const cashiers = await serverApiClient.get<GetCashiersResult[]>('/cashiers', traceContext);
		return { cashiers };
	}
}

export const invoiceBffService = new InvoiceBffService();