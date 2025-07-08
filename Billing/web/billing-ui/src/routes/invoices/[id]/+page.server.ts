import type { PageServerLoad, Actions } from './$types';
import { error, fail } from '@sveltejs/kit';
import { GetInvoiceQuery } from '$lib/invoices/actions/GetInvoiceQuery';
import { CancelInvoiceCommand } from '$lib/invoices/actions/CancelInvoiceCommand';
import { MarkInvoiceAsPaidCommand } from '$lib/invoices/actions/MarkInvoiceAsPaidCommand';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;

	if (!id) {
		throw error(400, {
			message: 'Invoice ID is required'
		});
	}

	try {
		const query = new GetInvoiceQuery(id);
		const invoice = await query.execute();

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
			const command = new CancelInvoiceCommand(id);
			await command.execute();
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

		try {
			const command = new MarkInvoiceAsPaidCommand(id, {
				amountPaid: amountPaid || 0,
				paymentDate: paymentDate || undefined
			});

			await command.execute();
			return { success: true };
		} catch (err) {
			if (err instanceof ValidationError) {
				return fail(400, {
					success: false,
					errorMessage: Object.values(err.errors)[0] || 'Validation failed'
				});
			}
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
	}
};
