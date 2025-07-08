import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail, isRedirect } from '@sveltejs/kit';
import { cashierApi } from '$lib/cashiers';
import { invoiceApi } from '$lib/invoices';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async () => {
	try {
		const cashiers = await cashierApi.getCashiers();

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

		// Server-side validation
		const errors: Record<string, string> = {};

		if (!name?.trim()) {
			errors.name = 'Name is required';
		}

		if (!amount || isNaN(amount) || amount <= 0) {
			errors.amount = 'Amount must be a positive number';
		}

		if (!currency) {
			errors.currency = 'Currency is required';
		}

		if (Object.keys(errors).length > 0) {
			return fail(400, {
				success: false,
				errors,
				values: { name, amount, currency, dueDate, cashierId }
			});
		}

		try {
			const createdInvoice = await invoiceApi.createInvoice({
				name: name.trim(),
				amount,
				currency,
				dueDate: dueDate || undefined,
				cashierId: cashierId || undefined
			});

			throw redirect(303, `/invoices/${createdInvoice.invoiceId}`);
		} catch (err: unknown) {
			if (isRedirect(err)) {
				throw err;
			}

			if (err instanceof ApiError) {
				console.error('Error creating invoice:', typeof err);
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
