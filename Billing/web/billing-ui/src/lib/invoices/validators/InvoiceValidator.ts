import type { CreateInvoiceRequest } from '../InvoiceApi';

export class ValidationError extends Error {
	constructor(public errors: Record<string, string>) {
		super('Validation failed');
		this.name = 'ValidationError';
	}
}

export class InvoiceValidator {
	static validateName(name: string | undefined | null): string | null {
		if (!name?.trim()) {
			return 'Invoice description is required';
		}
		
		const trimmedName = name.trim();
		
		if (trimmedName.length < 2) {
			return 'Description must be at least 2 characters';
		}
		
		if (trimmedName.length > 200) {
			return 'Description must not exceed 200 characters';
		}
		
		return null;
	}

	static validateAmount(amount: number | undefined | null): string | null {
		if (amount === undefined || amount === null) {
			return 'Amount is required';
		}
		
		if (amount <= 0) {
			return 'Amount must be greater than zero';
		}
		
		if (amount > 999999999) {
			return 'Amount exceeds maximum allowed value';
		}
		
		return null;
	}

	static validateCurrency(currency: string | undefined | null): string | null {
		if (!currency?.trim()) {
			return 'Currency is required';
		}
		
		const validCurrencies = ['USD', 'EUR', 'GBP', 'JPY', 'BRL'];
		if (!validCurrencies.includes(currency)) {
			return 'Invalid currency selected';
		}
		
		return null;
	}

	static validateDueDate(dueDate: string | undefined | null): string | null {
		if (!dueDate) {
			return 'Due date is required';
		}
		
		const dueDateObj = new Date(dueDate);
		const today = new Date();
		today.setHours(0, 0, 0, 0);
		
		if (isNaN(dueDateObj.getTime())) {
			return 'Invalid date format';
		}
		
		if (dueDateObj < today) {
			return 'Due date cannot be in the past';
		}
		
		return null;
	}

	static validateCashierId(cashierId: string | undefined | null): string | null {
		if (!cashierId?.trim()) {
			return 'Cashier is required';
		}
		
		return null;
	}

	static validateCreateRequest(data: CreateInvoiceRequest): Record<string, string> {
		const errors: Record<string, string> = {};
		
		const nameError = this.validateName(data.name);
		if (nameError) {
			errors.name = nameError;
		}
		
		const amountError = this.validateAmount(data.amount);
		if (amountError) {
			errors.amount = amountError;
		}
		
		const currencyError = this.validateCurrency(data.currency);
		if (currencyError) {
			errors.currency = currencyError;
		}
		
		const dueDateError = this.validateDueDate(data.dueDate);
		if (dueDateError) {
			errors.dueDate = dueDateError;
		}
		
		const cashierError = this.validateCashierId(data.cashierId);
		if (cashierError) {
			errors.cashierId = cashierError;
		}
		
		return errors;
	}

	static validateUpdateRequest(data: Partial<CreateInvoiceRequest>): Record<string, string> {
		const errors: Record<string, string> = {};
		
		if (data.name !== undefined) {
			const nameError = this.validateName(data.name);
			if (nameError) {
				errors.name = nameError;
			}
		}
		
		if (data.amount !== undefined) {
			const amountError = this.validateAmount(data.amount);
			if (amountError) {
				errors.amount = amountError;
			}
		}
		
		if (data.currency !== undefined) {
			const currencyError = this.validateCurrency(data.currency);
			if (currencyError) {
				errors.currency = currencyError;
			}
		}
		
		if (data.dueDate !== undefined) {
			const dueDateError = this.validateDueDate(data.dueDate);
			if (dueDateError) {
				errors.dueDate = dueDateError;
			}
		}
		
		if (data.cashierId !== undefined) {
			const cashierError = this.validateCashierId(data.cashierId);
			if (cashierError) {
				errors.cashierId = cashierError;
			}
		}
		
		return errors;
	}
}