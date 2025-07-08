import { InvoiceValidator, ValidationError } from '../validators/InvoiceValidator';
import { invoiceApi, type CreateInvoiceRequest, type Invoice as InvoiceDTO } from '../InvoiceApi';

export class UpdateInvoiceCommand {
	constructor(
		private readonly id: string,
		private readonly request: Partial<CreateInvoiceRequest>
	) {}

	async execute(): Promise<InvoiceDTO> {
		if (!this.id) {
			throw new Error('Invoice ID is required');
		}

		const validationErrors = InvoiceValidator.validateUpdateRequest(this.request);

		if (Object.keys(validationErrors).length > 0) {
			throw new ValidationError(validationErrors);
		}

		const normalizedRequest: Partial<CreateInvoiceRequest> = {};
		
		if (this.request.name !== undefined) {
			normalizedRequest.name = this.request.name.trim();
		}
		if (this.request.amount !== undefined) {
			normalizedRequest.amount = this.request.amount;
		}
		if (this.request.currency !== undefined) {
			normalizedRequest.currency = this.request.currency;
		}
		if (this.request.dueDate !== undefined) {
			normalizedRequest.dueDate = this.request.dueDate;
		}
		if (this.request.cashierId !== undefined) {
			normalizedRequest.cashierId = this.request.cashierId;
		}

		return await invoiceApi.updateInvoice(this.id, normalizedRequest);
	}
}