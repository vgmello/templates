import type { CashierData } from './models/cashier';
import type { Currency } from '$lib/core/values/Currency';

export class CashierService {
	validateCashierData(data: Partial<CashierData>): string[] {
		const errors: string[] = [];

		if (!data.name) {
			errors.push('Cashier name is required');
		}
		if (!data.email) {
			errors.push('Cashier email is required');
		}
		if (data.email && !this.isValidEmail(data.email)) {
			errors.push('Invalid email format');
		}
		if (!data.supportedCurrencies || data.supportedCurrencies.length === 0) {
			errors.push('At least one supported currency is required');
		}

		return errors;
	}

	private isValidEmail(email: string): boolean {
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		return emailRegex.test(email);
	}

	getCashiersForCurrency(cashiers: CashierData[], currency: Currency): CashierData[] {
		return cashiers.filter(
			(cashier) => cashier.isActive && cashier.supportedCurrencies.includes(currency)
		);
	}

	getActiveCashiers(cashiers: CashierData[]): CashierData[] {
		return cashiers.filter((cashier) => cashier.isActive);
	}

	formatCashierName(cashier: CashierData): string {
		return cashier.name || cashier.email || 'Unknown Cashier';
	}

	getCashierByEmail(cashiers: CashierData[], email: string): CashierData | null {
		return cashiers.find((cashier) => cashier.email === email) || null;
	}

	getCashierById(cashiers: CashierData[], id: string): CashierData | null {
		return cashiers.find((cashier) => cashier.id === id) || null;
	}

	createDefaultCashier(): Partial<CashierData> {
		return {
			name: '',
			email: '',
			phone: '',
			isActive: true,
			supportedCurrencies: ['USD']
		};
	}
}
