import { cashierApi, type UpdateCashierRequest, type Cashier as CashierDTO } from '../CashiersApi';

export class UpdateCashierCommand {
	constructor(
		private readonly id: string,
		private readonly request: UpdateCashierRequest
	) {}

	async execute(): Promise<CashierDTO> {
		if (!this.id?.trim()) {
			throw new Error('Cashier ID is required');
		}

		if (!this.request.name?.trim() || !this.request.email?.trim()) {
			throw new Error('Name and email are required');
		}

		return await cashierApi.updateCashier(this.id, this.request);
	}
}