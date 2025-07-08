import { cashierApi, type GetCashiersQuery as GetCashiersQueryParams, type GetCashiersResult } from '../CashiersApi';

export class GetCashiersQuery {
	constructor(private readonly query?: GetCashiersQueryParams) {}

	async execute(): Promise<GetCashiersResult[]> {
		return await cashierApi.getCashiers(this.query);
	}
}