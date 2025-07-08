import type { CreateCashierRequest } from '../CashiersApi';

export class CreateCashierForm {
	name = $state('');
	email = $state('');

	// Validation errors for UI feedback
	nameError = $derived.by(() => {
		if (!this.name.trim()) return 'Name is required';
		if (this.name.trim().length < 2) return 'Name must be at least 2 characters';
		return null;
	});

	emailError = $derived.by(() => {
		if (!this.email.trim()) return 'Email is required';
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		if (!emailRegex.test(this.email.trim())) return 'Please enter a valid email address';
		return null;
	});

	isValid = $derived(this.nameError === null && this.emailError === null);

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