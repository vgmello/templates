import { cashierApi, type CreateCashierRequest, type Cashier as CashierDTO } from '../CashiersApi';

export class CreateCashierCommand {
	constructor(private readonly request: CreateCashierRequest) {}

	async execute(): Promise<CashierDTO> {
		if (!this.request.name?.trim() || !this.request.email?.trim()) {
			throw new Error('Name and email are required');
		}

		return await cashierApi.createCashier(this.request);
	}
}
