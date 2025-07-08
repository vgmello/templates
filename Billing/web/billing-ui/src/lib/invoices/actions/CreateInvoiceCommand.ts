import { InvoiceValidator, ValidationError } from '../validators/InvoiceValidator';
import { invoiceApi, type CreateInvoiceRequest, type Invoice as InvoiceDTO } from '../InvoiceApi';

export class CreateInvoiceCommand {
	constructor(private readonly request: CreateInvoiceRequest) {}

	async execute(): Promise<InvoiceDTO> {
		const validationErrors = InvoiceValidator.validateCreateRequest(this.request);

		if (Object.keys(validationErrors).length > 0) {
			throw new ValidationError(validationErrors);
		}

		const normalizedRequest = {
			name: this.request.name.trim(),
			amount: this.request.amount,
			currency: this.request.currency || 'USD',
			dueDate: this.request.dueDate,
			cashierId: this.request.cashierId
		};

		return await invoiceApi.createInvoice(normalizedRequest);
	}
}