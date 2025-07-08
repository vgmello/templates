import type { CreateInvoiceRequest } from '../InvoiceApi';

export class CreateInvoiceForm {
	name = $state('');
	amount = $state('');
	currency = $state('USD');
	dueDate = $state('');
	cashierId = $state('');

	// Validation errors for UI feedback
	nameError = $derived.by(() => {
		if (!this.name.trim()) return 'Invoice name is required';
		if (this.name.trim().length < 3) return 'Invoice name must be at least 3 characters';
		return null;
	});

	amountError = $derived.by(() => {
		if (!this.amount.trim()) return 'Amount is required';
		const numAmount = parseFloat(this.amount);
		if (isNaN(numAmount)) return 'Amount must be a valid number';
		if (numAmount <= 0) return 'Amount must be greater than zero';
		return null;
	});

	currencyError = $derived.by(() => {
		if (!this.currency.trim()) return 'Currency is required';
		return null;
	});

	dueDateError = $derived.by(() => {
		if (!this.dueDate.trim()) return 'Due date is required';
		const date = new Date(this.dueDate);
		if (isNaN(date.getTime())) return 'Please enter a valid date';
		return null;
	});

	cashierError = $derived.by(() => {
		if (!this.cashierId || this.cashierId.trim() === '') return 'Cashier selection is required';
		return null;
	});

	isValid = $derived(
		this.nameError === null &&
		this.amountError === null &&
		this.currencyError === null &&
		this.dueDateError === null &&
		this.cashierError === null
	);

	toRequest(): CreateInvoiceRequest {
		return {
			name: this.name.trim(),
			amount: parseFloat(this.amount),
			currency: this.currency.trim(),
			dueDate: new Date(this.dueDate),
			cashierId: this.cashierId.trim()
		};
	}

	reset(): void {
		this.name = '';
		this.amount = '';
		this.currency = 'USD';
		this.dueDate = '';
		this.cashierId = '';
	}
}