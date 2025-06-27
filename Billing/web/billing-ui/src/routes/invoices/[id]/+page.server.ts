import { invoiceGrpcService } from '$lib/server/grpc-client';
import { error, fail, redirect } from '@sveltejs/kit';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ params }) => {
	try {
		const invoice = await invoiceGrpcService.getInvoice(params.id);
		
		return {
			invoice
		};
	} catch (err: any) {
		console.error('Failed to load invoice:', err);
		
		if (err.code === 5) { // NOT_FOUND
			throw error(404, 'Invoice not found');
		}
		
		throw error(500, 'Failed to load invoice');
	}
};

export const actions: Actions = {
	cancel: async ({ params }) => {
		try {
			await invoiceGrpcService.cancelInvoice(params.id);
			return { success: true, message: 'Invoice cancelled successfully' };
		} catch (err: any) {
			console.error('Failed to cancel invoice:', err);
			return fail(500, { error: 'Failed to cancel invoice' });
		}
	},

	markPaid: async ({ request, params }) => {
		const data = await request.formData();
		const amountPaid = parseFloat(data.get('amountPaid') as string);
		const paymentDate = data.get('paymentDate') as string;

		if (!amountPaid || amountPaid <= 0) {
			return fail(400, { error: 'Amount paid must be greater than 0' });
		}

		try {
			await invoiceGrpcService.markInvoiceAsPaid(params.id, {
				amountPaid,
				paymentDate: paymentDate || undefined
			});
			return { success: true, message: 'Invoice marked as paid' };
		} catch (err: any) {
			console.error('Failed to mark invoice as paid:', err);
			return fail(500, { error: 'Failed to mark invoice as paid' });
		}
	},

	simulatePayment: async ({ request, params }) => {
		const data = await request.formData();
		const amount = parseFloat(data.get('amount') as string);
		const currency = data.get('currency') as string;
		const paymentMethod = data.get('paymentMethod') as string;

		if (!amount || amount <= 0) {
			return fail(400, { error: 'Payment amount must be greater than 0' });
		}

		try {
			const result = await invoiceGrpcService.simulatePayment(params.id, {
				amount,
				currency: currency || 'USD',
				paymentMethod: paymentMethod || 'Credit Card'
			});
			return { success: true, message: result.message };
		} catch (err: any) {
			console.error('Failed to simulate payment:', err);
			return fail(500, { error: 'Failed to simulate payment' });
		}
	}
};