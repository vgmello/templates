import { invoiceApi, type MarkInvoiceAsPaidRequest } from '../InvoiceApi';

export class MarkInvoiceAsPaidCommand {
	constructor(
		private readonly id: string,
		private readonly request: MarkInvoiceAsPaidRequest
	) {}

	async execute(): Promise<void> {
		if (!this.id?.trim()) {
			throw new Error('Invoice ID is required');
		}

		if (!this.request.amountPaid || this.request.amountPaid <= 0) {
			throw new Error('Amount paid must be greater than zero');
		}

		return await invoiceApi.markInvoiceAsPaid(this.id, this.request);
	}
}