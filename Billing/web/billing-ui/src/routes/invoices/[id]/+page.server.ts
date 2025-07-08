import type { PageServerLoad, Actions } from './$types';
import { error, fail } from '@sveltejs/kit';
import { invoiceApi } from '$lib/api';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;

	if (!id) {
		throw error(400, {
			message: 'Invoice ID is required'
		});
	}

	try {
		const invoice = await invoiceApi.getInvoice(id);

		return {
			invoice
		};
	} catch (err) {
		console.error('Failed to load invoice:', err);

		if (err instanceof ApiError && err.status === 404) {
			throw error(404, {
				message: 'Invoice not found'
			});
		}

		throw error(500, {
			message: 'Failed to load invoice. Please try again later.'
		});
	}
};

export const actions: Actions = {
	cancel: async ({ params }) => {
		const { id } = params;

		if (!id) {
			return fail(400, {
				success: false,
				errorMessage: 'Invoice ID is required'
			});
		}

		try {
			await invoiceApi.cancelInvoice(id);
			return { success: true };
		} catch (err) {
			console.error('Failed to cancel invoice:', err);

			if (err instanceof ApiError && err.status === 404) {
				return fail(404, {
					success: false,
					errorMessage: 'Invoice not found'
				});
			}

			return fail(500, {
				success: false,
				errorMessage: 'Failed to cancel invoice. Please try again later.'
			});
		}
	},

	markPaid: async ({ params, request }) => {
		const { id } = params;

		if (!id) {
			return fail(400, {
				success: false,
				errorMessage: 'Invoice ID is required'
			});
		}

		const data = await request.formData();
		const amountPaid = parseFloat(data.get('amountPaid') as string);
		const paymentDate = data.get('paymentDate') as string;

		// Server-side validation
		if (!amountPaid || amountPaid <= 0) {
			return fail(400, {
				success: false,
				errorMessage: 'Amount paid must be greater than 0'
			});
		}

		try {
			await invoiceApi.markInvoiceAsPaid(id, {
				amountPaid,
				paymentDate: paymentDate || undefined
			});
			return { success: true };
		} catch (err) {
			console.error('Failed to mark invoice as paid:', err);

			if (err instanceof ApiError && err.status === 404) {
				return fail(404, {
					success: false,
					errorMessage: 'Invoice not found'
				});
			}

			return fail(500, {
				success: false,
				errorMessage: 'Failed to mark invoice as paid. Please try again later.'
			});
		}
	},

	simulatePayment: async ({ params, request }) => {
		const { id } = params;

		if (!id) {
			return fail(400, {
				success: false,
				errorMessage: 'Invoice ID is required'
			});
		}

		const data = await request.formData();
		const amount = parseFloat(data.get('amount') as string);
		const currency = data.get('currency') as string;
		const paymentMethod = data.get('paymentMethod') as string;
		const paymentReference = data.get('paymentReference') as string;

		// Server-side validation
		if (!amount || amount <= 0) {
			return fail(400, {
				success: false,
				errorMessage: 'Amount must be greater than 0'
			});
		}

		try {
			await invoiceApi.simulatePayment(id, {
				amount,
				currency: currency || undefined,
				paymentMethod: paymentMethod || undefined,
				paymentReference: paymentReference || undefined
			});
			return { success: true };
		} catch (err) {
			console.error('Failed to simulate payment:', err);

			if (err instanceof ApiError && err.status === 404) {
				return fail(404, {
					success: false,
					errorMessage: 'Invoice not found'
				});
			}

			return fail(500, {
				success: false,
				errorMessage: 'Failed to simulate payment. Please try again later.'
			});
		}
	}
};
