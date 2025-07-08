import { Money } from '../values/Money';
import { InvoiceStatuses, InvoiceStatusValue, type InvoiceStatus } from '../values/InvoiceStatus';
import type { Currency } from '../values/Currency';

export class Invoice {
	id = $state<string>('');
	number = $state<string>('');
	amount = $state<Money>(Money.zero('USD'));
	status = $state<InvoiceStatus>('draft');
	issueDate = $state<Date>(new Date());
	dueDate = $state<Date>(new Date());
	cashierId = $state<string | null>(null);
	cashierName = $state<string>('');
	customerName = $state<string>('');
	customerEmail = $state<string>('');
	description = $state<string>('');
	items = $state<InvoiceItem[]>([]);
	createdAt = $state<Date>(new Date());
	updatedAt = $state<Date>(new Date());

	statusValue = $derived(new InvoiceStatusValue(this.status));
	
	isOverdue = $derived(
		this.status === 'pending' && new Date() > this.dueDate
	);
	
	canBeCancelled = $derived(
		this.statusValue.canBeCancelled()
	);
	
	canBePaid = $derived(
		this.statusValue.canBePaid()
	);
	
	canBeEdited = $derived(
		this.statusValue.canBeEdited()
	);

	totalAmount = $derived(
		this.items.reduce(
			(sum, item) => sum.add(item.total),
			Money.zero(this.amount.currency)
		)
	);

	constructor(data?: Partial<InvoiceData>) {
		if (data) {
			this.updateFrom(data);
		}
	}

	updateFrom(data: Partial<InvoiceData>): void {
		if (data.id !== undefined) this.id = data.id;
		if (data.number !== undefined) this.number = data.number;
		if (data.amount !== undefined) this.amount = Money.fromCents(data.amount, data.currency || 'USD');
		if (data.status !== undefined) this.status = data.status;
		if (data.issueDate !== undefined) this.issueDate = new Date(data.issueDate);
		if (data.dueDate !== undefined) this.dueDate = new Date(data.dueDate);
		if (data.cashierId !== undefined) this.cashierId = data.cashierId;
		if (data.cashierName !== undefined) this.cashierName = data.cashierName;
		if (data.customerName !== undefined) this.customerName = data.customerName;
		if (data.customerEmail !== undefined) this.customerEmail = data.customerEmail;
		if (data.description !== undefined) this.description = data.description;
		if (data.createdAt !== undefined) this.createdAt = new Date(data.createdAt);
		if (data.updatedAt !== undefined) this.updatedAt = new Date(data.updatedAt);
		if (data.items !== undefined) {
			this.items = data.items.map(item => new InvoiceItem(item));
		}
	}

	cancel(): void {
		if (!this.canBeCancelled) {
			throw new Error('Invoice cannot be cancelled in its current status');
		}
		this.status = 'cancelled';
		this.updatedAt = new Date();
	}

	markPaid(): void {
		if (!this.canBePaid) {
			throw new Error('Invoice cannot be marked as paid in its current status');
		}
		this.status = 'paid';
		this.updatedAt = new Date();
	}

	addItem(item: InvoiceItem): void {
		if (!this.canBeEdited) {
			throw new Error('Invoice cannot be edited in its current status');
		}
		this.items = [...this.items, item];
		this.updatedAt = new Date();
	}

	removeItem(itemId: string): void {
		if (!this.canBeEdited) {
			throw new Error('Invoice cannot be edited in its current status');
		}
		this.items = this.items.filter(item => item.id !== itemId);
		this.updatedAt = new Date();
	}

	validate(): string[] {
		const errors: string[] = [];

		if (!this.customerName) {
			errors.push('Customer name is required');
		}
		if (!this.customerEmail) {
			errors.push('Customer email is required');
		}
		if (this.items.length === 0) {
			errors.push('At least one item is required');
		}
		if (this.dueDate < this.issueDate) {
			errors.push('Due date must be after issue date');
		}

		return errors;
	}
}

export class InvoiceItem {
	id = $state<string>('');
	description = $state<string>('');
	quantity = $state<number>(1);
	unitPrice = $state<Money>(Money.zero('USD'));
	
	total = $derived(
		this.unitPrice.multiply(this.quantity)
	);

	constructor(data?: Partial<InvoiceItemData>) {
		if (data) {
			if (data.id !== undefined) this.id = data.id;
			if (data.description !== undefined) this.description = data.description;
			if (data.quantity !== undefined) this.quantity = data.quantity;
			if (data.unitPrice !== undefined) {
				this.unitPrice = Money.fromCents(data.unitPrice, data.currency || 'USD');
			}
		}
	}
}

export interface InvoiceData {
	id: string;
	number: string;
	amount: number;
	currency: Currency;
	status: InvoiceStatus;
	issueDate: string;
	dueDate: string;
	cashierId: string | null;
	cashierName: string;
	customerName: string;
	customerEmail: string;
	description: string;
	items: InvoiceItemData[];
	createdAt: string;
	updatedAt: string;
}

export interface InvoiceItemData {
	id: string;
	description: string;
	quantity: number;
	unitPrice: number;
	currency: Currency;
}