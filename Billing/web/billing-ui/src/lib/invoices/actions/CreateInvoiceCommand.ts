import { invoiceApi, type CreateInvoiceRequest, type Invoice as InvoiceDTO } from '../InvoiceApi';

export class CreateInvoiceCommand {
	constructor(private readonly request: CreateInvoiceRequest) {}

	async execute(): Promise<InvoiceDTO> {
		if (!this.request.name?.trim() || !this.request.amount || !this.request.cashierId?.trim()) {
			throw new Error('Name, amount, and cashier ID are required');
		}

		if (this.request.amount <= 0) {
			throw new Error('Amount must be greater than zero');
		}

		return await invoiceApi.createInvoice(this.request);
	}
}