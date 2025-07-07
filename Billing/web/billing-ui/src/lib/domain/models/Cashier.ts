import type { Currency } from '../values/Currency';

export class Cashier {
	id = $state<string>('');
	name = $state<string>('');
	email = $state<string>('');
	phone = $state<string>('');
	isActive = $state<boolean>(true);
	supportedCurrencies = $state<Currency[]>([]);
	createdAt = $state<Date>(new Date());
	updatedAt = $state<Date>(new Date());

	displayName = $derived(
		this.name || this.email || 'Unknown Cashier'
	);

	canHandleCurrency = $derived(
		(currency: Currency) => this.supportedCurrencies.includes(currency)
	);

	constructor(data?: Partial<CashierData>) {
		if (data) {
			this.updateFrom(data);
		}
	}

	updateFrom(data: Partial<CashierData>): void {
		if (data.id !== undefined) this.id = data.id;
		if (data.name !== undefined) this.name = data.name;
		if (data.email !== undefined) this.email = data.email;
		if (data.phone !== undefined) this.phone = data.phone;
		if (data.isActive !== undefined) this.isActive = data.isActive;
		if (data.supportedCurrencies !== undefined) {
			this.supportedCurrencies = [...data.supportedCurrencies];
		}
		if (data.createdAt !== undefined) this.createdAt = new Date(data.createdAt);
		if (data.updatedAt !== undefined) this.updatedAt = new Date(data.updatedAt);
	}

	activate(): void {
		this.isActive = true;
		this.updatedAt = new Date();
	}

	deactivate(): void {
		this.isActive = false;
		this.updatedAt = new Date();
	}

	addSupportedCurrency(currency: Currency): void {
		if (!this.supportedCurrencies.includes(currency)) {
			this.supportedCurrencies = [...this.supportedCurrencies, currency];
			this.updatedAt = new Date();
		}
	}

	removeSupportedCurrency(currency: Currency): void {
		this.supportedCurrencies = this.supportedCurrencies.filter(c => c !== currency);
		this.updatedAt = new Date();
	}

	validate(): string[] {
		const errors: string[] = [];

		if (!this.name) {
			errors.push('Cashier name is required');
		}
		if (!this.email) {
			errors.push('Cashier email is required');
		}
		if (this.email && !this.isValidEmail(this.email)) {
			errors.push('Invalid email format');
		}
		if (this.supportedCurrencies.length === 0) {
			errors.push('At least one supported currency is required');
		}

		return errors;
	}

	private isValidEmail(email: string): boolean {
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		return emailRegex.test(email);
	}
}

export interface CashierData {
	id: string;
	name: string;
	email: string;
	phone: string;
	isActive: boolean;
	supportedCurrencies: Currency[];
	createdAt: string;
	updatedAt: string;
}