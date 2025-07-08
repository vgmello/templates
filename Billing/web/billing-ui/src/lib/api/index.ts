// Re-export from feature modules for backward compatibility
export { cashierApi } from '$lib/cashiers';
export { invoiceApi } from '$lib/invoices';

// Types
export type {
	GetCashiersResult,
	CreateCashierRequest,
	UpdateCashierRequest,
	GetCashiersQuery
} from '$lib/cashiers';

export type {
	Invoice as InvoiceDTO,
	CreateInvoiceRequest,
	MarkInvoiceAsPaidRequest,
	SimulatePaymentRequest,
	GetInvoicesQuery
} from '$lib/invoices';
