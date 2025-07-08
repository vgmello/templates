// Re-export from new locations for backward compatibility
// Models
export { Invoice, type InvoiceData } from '$lib/invoices';
export { Cashier, type CashierData } from '$lib/cashiers';

// Value Objects - now in core
export { Money } from '$lib/core/values/Money';
export {
	InvoiceStatuses,
	InvoiceStatusValue,
	type InvoiceStatus
} from '$lib/core/values/InvoiceStatus';
export { SupportedCurrencies, CurrencyValue, type Currency } from '$lib/core/values/Currency';

// Services
export { InvoiceService, type InvoiceSummary } from '$lib/invoices';
export { CashierService } from '$lib/cashiers';
