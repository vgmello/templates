import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail, isRedirect } from '@sveltejs/kit';
import { GetCashiersQuery } from '$lib/cashiers';
import { CreateInvoiceCommand } from '$lib/invoices/actions/CreateInvoiceCommand';
import { ValidationError } from '$lib/invoices/validators/InvoiceValidator';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async () => {
	try {
		const query = new GetCashiersQuery();
		const cashiers = await query.execute();

		return {
			cashiers
		};
	} catch (err) {
		console.error('Failed to load invoice create data:', err);
		throw error(500, {
			message: 'Failed to load required data. Please try again later.'
		});
	}
};

export const actions: Actions = {
	default: async ({ request }) => {
		const data = await request.formData();

		const name = data.get('name') as string;
		const amount = parseFloat(data.get('amount') as string);
		const currency = data.get('currency') as string;
		const dueDate = data.get('dueDate') as string;
		const cashierId = data.get('cashierId') as string;

		try {
			const command = new CreateInvoiceCommand({
				name: name || '',
				amount: amount || 0,
				currency: currency || '',
				dueDate: dueDate || undefined,
				cashierId: cashierId || undefined
			});

			const createdInvoice = await command.execute();

			throw redirect(303, `/invoices/${createdInvoice.invoiceId}`);
		} catch (err: unknown) {
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

			console.error('Failed to create invoice:', err);

			if (err instanceof ApiError && err.status === 400) {
				return fail(400, {
					success: false,
					errors: { form: err.message },
					values: { name, amount, currency, dueDate, cashierId }
				});
			}

			return fail(500, {
				success: false,
				errors: { form: 'Failed to create invoice. Please try again later.' },
				values: { name, amount, currency, dueDate, cashierId }
			});
		}
	}
};
