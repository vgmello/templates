import type { CreateInvoiceRequest } from '../InvoiceApi';

export class CreateInvoiceForm {
	name = $state('');
	amount = $state('');
	currency = $state('USD');
	dueDate = $state('');
	cashierId = $state('');

	isValid = $derived(
		this.name.trim() !== '' &&
		this.amount.trim() !== '' &&
		!isNaN(parseFloat(this.amount)) &&
		parseFloat(this.amount) > 0 &&
		this.currency.trim() !== '' &&
		this.dueDate.trim() !== '' &&
		this.cashierId.trim() !== ''
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