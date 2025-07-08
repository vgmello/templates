import { ValidationError } from '../CashierService';
import { cashierApi, type CreateCashierRequest, type Cashier as CashierDTO } from '../CashiersApi';

export class CreateCashierCommand {
	constructor(private readonly request: CreateCashierRequest) {}

	async execute(): Promise<CashierDTO> {
		const validationErrors = this.validateCreateRequest(this.request);

		if (Object.keys(validationErrors).length > 0) {
			throw new ValidationError(validationErrors);
		}

		const normalizedRequest = {
			name: this.request.name.trim(),
			email: this.request.email.trim()
		};

		return await cashierApi.createCashier(normalizedRequest);
	}

	private validateCreateRequest(data: CreateCashierRequest): Record<string, string> {
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

	private isValidEmail(email: string): boolean {
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		return emailRegex.test(email);
	}
}
