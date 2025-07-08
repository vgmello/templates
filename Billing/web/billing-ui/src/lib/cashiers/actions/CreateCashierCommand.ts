import { CashierValidator, ValidationError } from '../validators/CashierValidator';
import { cashierApi, type CreateCashierRequest, type Cashier as CashierDTO } from '../CashiersApi';

export class CreateCashierCommand {
	constructor(private readonly request: CreateCashierRequest) {}

	async execute(): Promise<CashierDTO> {
		const validationErrors = CashierValidator.validateCreateRequest(this.request);

		if (Object.keys(validationErrors).length > 0) {
			throw new ValidationError(validationErrors);
		}

		const normalizedRequest = {
			name: this.request.name.trim(),
			email: this.request.email.trim()
		};

		return await cashierApi.createCashier(normalizedRequest);
	}
}
