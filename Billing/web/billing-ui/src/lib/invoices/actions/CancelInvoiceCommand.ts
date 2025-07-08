import { invoiceApi } from '../InvoiceApi';

export class CancelInvoiceCommand {
	constructor(private readonly id: string) {}

	async execute(): Promise<void> {
		if (!this.id?.trim()) {
			throw new Error('Invoice ID is required');
		}

		return await invoiceApi.cancelInvoice(this.id);
	}
}