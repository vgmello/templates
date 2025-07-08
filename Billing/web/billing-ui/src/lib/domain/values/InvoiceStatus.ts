export const InvoiceStatuses = {
	DRAFT: 'draft',
	PENDING: 'pending',
	PAID: 'paid',
	CANCELLED: 'cancelled',
	OVERDUE: 'overdue'
} as const;

export type InvoiceStatus = (typeof InvoiceStatuses)[keyof typeof InvoiceStatuses];

export class InvoiceStatusValue {
	constructor(public readonly value: InvoiceStatus) {
		if (!Object.values(InvoiceStatuses).includes(value)) {
			throw new Error(`Invalid invoice status: ${value}`);
		}
	}

	isDraft(): boolean {
		return this.value === InvoiceStatuses.DRAFT;
	}

	isPending(): boolean {
		return this.value === InvoiceStatuses.PENDING;
	}

	isPaid(): boolean {
		return this.value === InvoiceStatuses.PAID;
	}

	isCancelled(): boolean {
		return this.value === InvoiceStatuses.CANCELLED;
	}

	isOverdue(): boolean {
		return this.value === InvoiceStatuses.OVERDUE;
	}

	canBeCancelled(): boolean {
		return this.isDraft() || this.isPending();
	}

	canBePaid(): boolean {
		return this.isPending() || this.isOverdue();
	}

	canBeEdited(): boolean {
		return this.isDraft();
	}

	getDisplayName(): string {
		const displayNames: Record<InvoiceStatus, string> = {
			draft: 'Draft',
			pending: 'Pending',
			paid: 'Paid',
			cancelled: 'Cancelled',
			overdue: 'Overdue'
		};
		return displayNames[this.value];
	}

	getDisplayColor(): string {
		const colors: Record<InvoiceStatus, string> = {
			draft: 'gray',
			pending: 'yellow',
			paid: 'green',
			cancelled: 'red',
			overdue: 'orange'
		};
		return colors[this.value];
	}
}