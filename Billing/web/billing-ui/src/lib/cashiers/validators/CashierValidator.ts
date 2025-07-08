import type { CreateCashierRequest, UpdateCashierRequest } from '../CashiersApi';

export class ValidationError extends Error {
	constructor(public errors: Record<string, string>) {
		super('Validation failed');
		this.name = 'ValidationError';
	}
}

export class CashierValidator {
	static validateName(name: string | undefined | null): string | null {
		if (!name?.trim()) {
			return 'Name is required';
		}
		
		const trimmedName = name.trim();
		
		if (trimmedName.length < 2) {
			return 'Name must be at least 2 characters';
		}
		
		if (trimmedName.length > 100) {
			return 'Name must not exceed 100 characters';
		}
		
		return null;
	}

	static validateEmail(email: string | undefined | null): string | null {
		if (!email) {
			return null; // Email is optional
		}
		
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		if (!emailRegex.test(email)) {
			return 'Please enter a valid email address';
		}
		
		return null;
	}

	static validateCreateRequest(data: CreateCashierRequest): Record<string, string> {
		const errors: Record<string, string> = {};
		
		const nameError = this.validateName(data.name);
		if (nameError) {
			errors.name = nameError;
		}
		
		const emailError = this.validateEmail(data.email);
		if (emailError) {
			errors.email = emailError;
		}
		
		return errors;
	}

	static validateUpdateRequest(data: UpdateCashierRequest): Record<string, string> {
		const errors: Record<string, string> = {};
		
		const nameError = this.validateName(data.name);
		if (nameError) {
			errors.name = nameError;
		}
		
		const emailError = this.validateEmail(data.email);
		if (emailError) {
			errors.email = emailError;
		}
		
		return errors;
	}
}