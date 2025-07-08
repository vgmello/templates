import { invoiceApi, type Invoice as InvoiceDTO } from '../InvoiceApi';

export class GetInvoiceQuery {
	constructor(private readonly id: string) {}

	async execute(): Promise<InvoiceDTO> {
		if (!this.id) {
			throw new Error('Invoice ID is required');
		}

		return await invoiceApi.getInvoice(this.id);
	}
}