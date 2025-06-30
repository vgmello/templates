// Invoice types based on Billing.Contracts.Invoices.Models.Invoice

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
}

// Invoice status constants
export const InvoiceStatus = {
	DRAFT: 'Draft',
	PENDING: 'Pending',
	PAID: 'Paid',
	CANCELLED: 'Cancelled',
	OVERDUE: 'Overdue'
} as const;

export type InvoiceStatusType = typeof InvoiceStatus[keyof typeof InvoiceStatus];

// Invoice summary for dashboard
export interface InvoiceSummary {
	totalInvoices: number;
	totalAmount: number;
	paidCount: number;
	overdueCount: number;
}