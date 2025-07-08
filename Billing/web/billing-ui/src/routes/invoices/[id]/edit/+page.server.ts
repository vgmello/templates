import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail, isRedirect } from '@sveltejs/kit';
import { invoiceApi } from '$lib/invoices';
import { cashierApi } from '$lib/cashiers';
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
			invoiceApi.getInvoice(id),
			cashierApi.getCashiers()
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

		// Server-side validation
		const errors: Record<string, string> = {};

		if (!name?.trim()) {
			errors.name = 'Invoice name is required';
		} else if (name.trim().length < 2) {
			errors.name = 'Invoice name must be at least 2 characters';
		} else if (name.trim().length > 200) {
			errors.name = 'Invoice name must not exceed 200 characters';
		}

		if (!amount || isNaN(Number(amount)) || Number(amount) <= 0) {
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
			await invoiceApi.updateInvoice(id, {
				name: name.trim(),
				amount: Number(amount),
				currency: currency || 'USD',
				dueDate: dueDate || undefined,
				cashierId: cashierId || undefined
			});

			throw redirect(303, `/invoices/${id}`);
		} catch (err) {
			if (isRedirect(err)) {
				throw err;
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
