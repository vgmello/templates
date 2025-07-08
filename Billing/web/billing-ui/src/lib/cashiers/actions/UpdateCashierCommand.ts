import { CashierValidator, ValidationError } from '../validators/CashierValidator';
import { cashierApi, type UpdateCashierRequest, type Cashier as CashierDTO } from '../CashiersApi';

export class UpdateCashierCommand {
	constructor(
		private readonly id: string,
		private readonly request: UpdateCashierRequest
	) {}

	async execute(): Promise<CashierDTO> {
		if (!this.id) {
			throw new Error('Cashier ID is required');
		}

		const validationErrors = CashierValidator.validateUpdateRequest(this.request);

		if (Object.keys(validationErrors).length > 0) {
			throw new ValidationError(validationErrors);
		}

		const normalizedRequest = {
			...this.request,
			name: this.request.name.trim(),
			email: this.request.email.trim()
		};

		return await cashierApi.updateCashier(this.id, normalizedRequest);
	}
}