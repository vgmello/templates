import {
	invoiceApi,
	type GetInvoicesQuery as GetInvoicesQueryParams,
	type Invoice as InvoiceDTO
} from '../InvoiceApi';

export class GetInvoicesQuery {
	constructor(private readonly query?: GetInvoicesQueryParams) {}

	async execute(): Promise<InvoiceDTO[]> {
		return await invoiceApi.getInvoices(this.query);
	}
}
