import type { UpdateCashierRequest, Cashier } from '../CashiersApi';

export class UpdateCashierForm {
	name = $state('');
	email = $state('');

	isValid = $derived(this.name.trim() !== '' && this.email.trim() !== '');

	loadFrom(cashier: Cashier): void {
		this.name = cashier.name;
		this.email = cashier.email;
	}

	toRequest(): UpdateCashierRequest {
		return {
			name: this.name.trim(),
			email: this.email.trim()
		};
	}

	reset(): void {
		this.name = '';
		this.email = '';
	}
}