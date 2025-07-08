import type { InvoiceData } from '../models/Invoice';
import { Money } from '../values/Money';
import type { Currency } from '../values/Currency';

export class InvoiceService {
	calculateSummary(invoices: InvoiceData[]): InvoiceSummary {
		const groupedByCurrency = this.groupInvoicesByCurrency(invoices);
		const summaries: CurrencySummary[] = [];

		for (const [currency, currencyInvoices] of Object.entries(groupedByCurrency)) {
			const summary = this.calculateCurrencySummary(currencyInvoices, currency as Currency);
			summaries.push(summary);
		}

		return {
			totalInvoices: invoices.length,
			currencySummaries: summaries,
			lastUpdated: new Date()
		};
	}

	private groupInvoicesByCurrency(invoices: InvoiceData[]): Record<string, InvoiceData[]> {
		return invoices.reduce(
			(acc, invoice) => {
				const currency = invoice.currency;
				if (!acc[currency]) {
					acc[currency] = [];
				}
				acc[currency].push(invoice);
				return acc;
			},
			{} as Record<string, InvoiceData[]>
		);
	}

	private calculateCurrencySummary(invoices: InvoiceData[], currency: Currency): CurrencySummary {
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
			const amount = Money.fromCents(invoice.amount, currency);
			const isOverdue = invoice.status === 'pending' && new Date(invoice.dueDate) < now;
			const effectiveStatus = isOverdue ? 'overdue' : invoice.status;

			counts[effectiveStatus]++;
			amounts[effectiveStatus] = amounts[effectiveStatus]?.add(amount);
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
