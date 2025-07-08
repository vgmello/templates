import { invoiceApi, type CreateInvoiceRequest, type Invoice as InvoiceDTO } from '../InvoiceApi';

export class UpdateInvoiceCommand {
	constructor(
		private readonly id: string,
		private readonly request: Partial<CreateInvoiceRequest>
	) {}

	async execute(): Promise<InvoiceDTO> {
		if (!this.id?.trim()) {
			throw new Error('Invoice ID is required');
		}

		if (this.request.amount !== undefined && this.request.amount <= 0) {
			throw new Error('Amount must be greater than zero');
		}

		return await invoiceApi.updateInvoice(this.id, this.request);
	}
}