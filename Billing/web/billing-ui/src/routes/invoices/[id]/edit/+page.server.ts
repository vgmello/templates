import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail, isRedirect } from '@sveltejs/kit';
import { GetInvoiceQuery } from '$lib/invoices/actions/GetInvoiceQuery';
import { UpdateInvoiceCommand } from '$lib/invoices/actions/UpdateInvoiceCommand';
import { GetCashiersQuery } from '$lib/cashiers';
import { ValidationError } from '$lib/invoices/validators/InvoiceValidator';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;

	if (!id) {
		throw error(400, {
			message: 'Invoice ID is required'
		});
	}

	try {
		// Load invoice and cashiers in parallel for better performance
		const [invoice, cashiers] = await Promise.all([
			new GetInvoiceQuery(id).execute(),
			new GetCashiersQuery().execute()
		]);

		return {
			invoice,
			cashiers
		};
	} catch (err) {
		console.error('Failed to load invoice or cashiers:', err);

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
	default: async ({ params, request }) => {
		const { id } = params;

		if (!id) {
			return fail(400, {
				success: false,
				errors: { form: 'Invoice ID is required' },
				values: {}
			});
		}

		const data = await request.formData();

		const name = data.get('name') as string;
		const amount = data.get('amount') as string;
		const currency = data.get('currency') as string;
		const dueDate = data.get('dueDate') as string;
		const cashierId = data.get('cashierId') as string;

		try {
			const command = new UpdateInvoiceCommand(id, {
				name: name || undefined,
				amount: amount ? Number(amount) : undefined,
				currency: currency || undefined,
				dueDate: dueDate || undefined,
				cashierId: cashierId || undefined
			});

			await command.execute();

			throw redirect(303, `/invoices/${id}`);
		} catch (err) {
			if (isRedirect(err)) {
				throw err;
			}

			if (err instanceof ValidationError) {
				return fail(400, {
					success: false,
					errors: err.errors,
					values: { name, amount, currency, dueDate, cashierId }
				});
			}

			console.error('Failed to update invoice:', err);

			if (err instanceof ApiError && err.status === 404) {
				return fail(404, {
					success: false,
					errors: { form: 'Invoice not found' },
					values: { name, amount, currency, dueDate, cashierId }
				});
			}

			if (err instanceof ApiError && err.status === 400) {
				return fail(400, {
					success: false,
					errors: { form: err.message },
					values: { name, amount, currency, dueDate, cashierId }
				});
			}

			return fail(500, {
				success: false,
				errors: { form: 'Failed to update invoice. Please try again later.' },
				values: { name, amount, currency, dueDate, cashierId }
			});
		}
	}
};
