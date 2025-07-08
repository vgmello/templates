import { cashierApi } from '../CashiersApi';

export class DeleteCashierCommand {
	constructor(private readonly id: string) {}

	async execute(): Promise<void> {
		if (!this.id?.trim()) {
			throw new Error('Cashier ID is required');
		}

		return await cashierApi.deleteCashier(this.id);
	}
}
