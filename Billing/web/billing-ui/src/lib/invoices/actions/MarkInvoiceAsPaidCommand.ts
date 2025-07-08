import { invoiceApi, type MarkInvoiceAsPaidRequest } from '../InvoiceApi';
import { ValidationError } from '../validators/InvoiceValidator';

export class MarkInvoiceAsPaidCommand {
	constructor(
		private readonly id: string,
		private readonly request: MarkInvoiceAsPaidRequest
	) {}

	async execute(): Promise<void> {
		if (!this.id) {
			throw new Error('Invoice ID is required');
		}

		const errors: Record<string, string> = {};

		if (!this.request.amountPaid || this.request.amountPaid <= 0) {
			errors.amountPaid = 'Amount paid must be greater than zero';
		}

		if (this.request.paymentDate) {
			const paymentDateObj = new Date(this.request.paymentDate);
			if (isNaN(paymentDateObj.getTime())) {
				errors.paymentDate = 'Invalid payment date format';
			}
		}

		if (Object.keys(errors).length > 0) {
			throw new ValidationError(errors);
		}

		return await invoiceApi.markInvoiceAsPaid(this.id, this.request);
	}
}