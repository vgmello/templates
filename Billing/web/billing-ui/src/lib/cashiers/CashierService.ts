import type { CashierData } from './models/Cashier';
import type { Currency } from '$lib/core/values/Currency';
import {
	cashierApi,
	type UpdateCashierRequest,
	type GetCashiersQuery,
	type GetCashiersResult,
	type Cashier as CashierDTO
} from './CashiersApi';

export class ValidationError extends Error {
	constructor(public errors: Record<string, string>) {
		super('Validation failed');
		this.name = 'ValidationError';
	}
}

export class CashierService {
	// CRUD operations with integrated validation
	async getCashiers(query?: GetCashiersQuery): Promise<GetCashiersResult[]> {
		return await cashierApi.getCashiers(query);
	}

	async updateCashier(id: string, request: UpdateCashierRequest): Promise<CashierDTO> {
		if (!id) {
			throw new Error('Cashier ID is required');
		}

		// Validate the request first
		const validationErrors = this.validateUpdateRequest(request);
		if (Object.keys(validationErrors).length > 0) {
			throw new ValidationError(validationErrors);
		}

		// Trim and normalize data
		const normalizedRequest = {
			...request,
			name: request.name.trim(),
			email: request.email.trim()
		};

		return await cashierApi.updateCashier(id, normalizedRequest);
	}

	async deleteCashier(id: string): Promise<void> {
		if (!id) {
			throw new Error('Cashier ID is required');
		}
		return await cashierApi.deleteCashier(id);
	}

	validateUpdateRequest(data: UpdateCashierRequest): Record<string, string> {
		const errors: Record<string, string> = {};

		if (!data.name?.trim()) {
			errors.name = 'Name is required';
		} else if (data.name.trim().length < 2) {
			errors.name = 'Name must be at least 2 characters';
		} else if (data.name.trim().length > 100) {
			errors.name = 'Name must not exceed 100 characters';
		}

		if (data.email && !this.isValidEmail(data.email)) {
			errors.email = 'Please enter a valid email address';
		}

		return errors;
	}

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
