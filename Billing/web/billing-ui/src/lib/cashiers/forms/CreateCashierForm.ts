import type { CreateCashierRequest } from '../CashiersApi';

export class CreateCashierForm {
	name = $state('');
	email = $state('');

	isValid = $derived(this.name.trim() !== '' && this.email.trim() !== '');

	toRequest(): CreateCashierRequest {
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