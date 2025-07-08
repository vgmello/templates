import type { InvoiceData } from './models/Invoice';

export class InvoiceService {

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
