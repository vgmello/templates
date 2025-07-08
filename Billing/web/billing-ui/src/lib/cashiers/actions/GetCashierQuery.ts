import { cashierApi, type Cashier as CashierDTO } from '../CashiersApi';

export class GetCashierQuery {
	constructor(private readonly id: string) {}

	async execute(): Promise<CashierDTO> {
		if (!this.id) {
			throw new Error('Cashier ID is required');
		}

		return await cashierApi.getCashier(this.id);
	}
}
