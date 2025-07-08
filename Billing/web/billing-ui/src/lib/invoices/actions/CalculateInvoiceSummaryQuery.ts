import type { Invoice } from '../InvoiceApi';
import { Money } from '$lib/core/values/Money';
import type { Currency } from '$lib/core/values/Currency';

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

export class CalculateInvoiceSummaryQuery {
	constructor(private readonly invoices: Invoice[]) {}

	execute(): InvoiceSummary {
		const groupedByCurrency = this.groupInvoicesByCurrency(this.invoices);
		const currencySummaries: CurrencySummary[] = [];

		for (const [currency, invoices] of Object.entries(groupedByCurrency)) {
			currencySummaries.push(this.calculateCurrencySummary(invoices, currency as Currency));
		}

		return {
			totalInvoices: this.invoices.length,
			currencySummaries,
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

			// Count and sum by status
			switch (effectiveStatus) {
				case 'draft':
					counts.draft++;
					amounts.draft = amounts.draft.add(amount);
					break;
				case 'pending':
					counts.pending++;
					amounts.pending = amounts.pending.add(amount);
					break;
				case 'paid':
					counts.paid++;
					amounts.paid = amounts.paid.add(amount);
					break;
				case 'cancelled':
					counts.cancelled++;
					amounts.cancelled = amounts.cancelled.add(amount);
					break;
				case 'overdue':
					counts.overdue++;
					amounts.overdue = amounts.overdue.add(amount);
					break;
				default:
					// Fallback to draft for invalid statuses
					console.warn(
						`Invalid invoice status: ${effectiveStatus}, defaulting to 'draft'`
					);
					counts.draft++;
					amounts.draft = amounts.draft.add(amount);
					break;
			}
			amounts.total = amounts.total.add(amount);
		}

		return {
			currency,
			counts,
			amounts
		};
	}
}
