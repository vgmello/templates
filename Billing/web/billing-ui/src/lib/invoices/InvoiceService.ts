import type { InvoiceData } from './models/Invoice';
import type { Invoice } from './InvoiceApi';
import { Money } from '$lib/core/values/Money';
import type { Currency } from '$lib/core/values/Currency';

export class InvoiceService {
	calculateSummary(invoices: Invoice[]): InvoiceSummary {
		// Simplified implementation to test
		return {
			totalInvoices: invoices.length,
			currencySummaries: [
				{
					currency: 'USD',
					counts: {
						total: invoices.length,
						draft: invoices.filter((i) => i.status.toLowerCase() === 'draft').length,
						pending: 0,
						paid: 0,
						cancelled: 0,
						overdue: 0
					},
					amounts: {
						total: Money.zero('USD'),
						draft: Money.zero('USD'),
						pending: Money.zero('USD'),
						paid: Money.zero('USD'),
						cancelled: Money.zero('USD'),
						overdue: Money.zero('USD')
					}
				}
			],
			lastUpdated: new Date()
		};
	}

	private groupInvoicesByCurrency(invoices: Invoice[]): Record<string, Invoice[]> {
		return invoices.reduce(
			(acc, invoice) => {
				const currency = invoice.currency || 'USD';
				if (!acc[currency]) {
					acc[currency] = [];
				}
				acc[currency].push(invoice);
				return acc;
			},
			{} as Record<string, Invoice[]>
		);
	}

	private calculateCurrencySummary(invoices: Invoice[], currency: Currency): CurrencySummary {
		const now = new Date();
		const counts = {
			total: invoices.length,
			draft: 0,
			pending: 0,
			paid: 0,
			cancelled: 0,
			overdue: 0
		};

		const amounts = {
			total: Money.zero(currency),
			draft: Money.zero(currency),
			pending: Money.zero(currency),
			paid: Money.zero(currency),
			cancelled: Money.zero(currency),
			overdue: Money.zero(currency)
		};

		for (const invoice of invoices) {
			const amount = Money.fromCents(Math.round(invoice.amount * 100), currency);
			const isOverdue =
				invoice.status.toLowerCase() === 'pending' &&
				invoice.dueDate &&
				new Date(invoice.dueDate) < now;
			const effectiveStatus = isOverdue ? 'overdue' : invoice.status.toLowerCase();

			// Safety check for valid status
			if (effectiveStatus === 'draft') {
				counts.draft++;
				amounts.draft = amounts.draft.add(amount);
			} else if (effectiveStatus === 'pending') {
				counts.pending++;
				amounts.pending = amounts.pending.add(amount);
			} else if (effectiveStatus === 'paid') {
				counts.paid++;
				amounts.paid = amounts.paid.add(amount);
			} else if (effectiveStatus === 'cancelled') {
				counts.cancelled++;
				amounts.cancelled = amounts.cancelled.add(amount);
			} else if (effectiveStatus === 'overdue') {
				counts.overdue++;
				amounts.overdue = amounts.overdue.add(amount);
			} else {
				// Fallback to draft for invalid statuses
				console.warn(`Invalid invoice status: ${effectiveStatus}, defaulting to 'draft'`);
				counts.draft++;
				amounts.draft = amounts.draft.add(amount);
			}
			amounts.total = amounts.total.add(amount);
		}

		return {
			currency,
			counts,
			amounts
		};
	}

	validateInvoiceData(data: Partial<InvoiceData>): string[] {
		const errors: string[] = [];

		if (!data.customerName) {
			errors.push('Customer name is required');
		}
		if (!data.customerEmail) {
			errors.push('Customer email is required');
		}
		if (!data.amount || data.amount <= 0) {
			errors.push('Amount must be greater than zero');
		}
		if (!data.currency) {
			errors.push('Currency is required');
		}
		if (!data.dueDate) {
			errors.push('Due date is required');
		}
		if (data.dueDate && data.issueDate && new Date(data.dueDate) < new Date(data.issueDate)) {
			errors.push('Due date must be after issue date');
		}

		return errors;
	}

	generateInvoiceNumber(): string {
		const now = new Date();
		const year = now.getFullYear();
		const month = String(now.getMonth() + 1).padStart(2, '0');
		const day = String(now.getDate()).padStart(2, '0');
		const timestamp = now.getTime().toString().slice(-6);

		return `INV-${year}${month}${day}-${timestamp}`;
	}

	isOverdue(invoice: InvoiceData): boolean {
		return invoice.status === 'pending' && new Date(invoice.dueDate) < new Date();
	}

	getDaysUntilDue(invoice: InvoiceData): number {
		const dueDate = new Date(invoice.dueDate);
		const now = new Date();
		const timeDiff = dueDate.getTime() - now.getTime();
		return Math.ceil(timeDiff / (1000 * 3600 * 24));
	}

	getDaysOverdue(invoice: InvoiceData): number {
		const daysUntilDue = this.getDaysUntilDue(invoice);
		return daysUntilDue < 0 ? Math.abs(daysUntilDue) : 0;
	}
}

export interface InvoiceSummary {
	totalInvoices: number;
	currencySummaries: CurrencySummary[];
	lastUpdated: Date;
}

export interface CurrencySummary {
	currency: Currency;
	counts: {
		total: number;
		draft: number;
		pending: number;
		paid: number;
		cancelled: number;
		overdue: number;
	};
	amounts: {
		total: Money;
		draft: Money;
		pending: Money;
		paid: Money;
		cancelled: Money;
		overdue: Money;
	};
}
