import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail, isRedirect } from '@sveltejs/kit';
import { GetCashiersQuery } from '$lib/cashiers';
import { CreateInvoiceCommand } from '$lib/invoices/actions/CreateInvoiceCommand';
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

		// Safely extract and validate form data
		const nameRaw = data.get('name');
		const amountRaw = data.get('amount');
		const currencyRaw = data.get('currency');
		const dueDateRaw = data.get('dueDate');
		const cashierIdRaw = data.get('cashierId');

		// Input validation
		const errors: Record<string, string> = {};

		// Validate name
		const name = typeof nameRaw === 'string' ? nameRaw.trim() : '';
		if (!name) {
			errors.name = 'Invoice name is required';
		} else if (name.length < 3) {
			errors.name = 'Invoice name must be at least 3 characters';
		}

		// Validate amount
		let amount = 0;
		if (typeof amountRaw !== 'string' || !amountRaw.trim()) {
			errors.amount = 'Amount is required';
		} else {
			amount = parseFloat(amountRaw);
			if (isNaN(amount) || !isFinite(amount)) {
				errors.amount = 'Amount must be a valid number';
			} else if (amount <= 0) {
				errors.amount = 'Amount must be greater than zero';
			}
		}

		// Validate currency
		const currency = typeof currencyRaw === 'string' ? currencyRaw.trim() : '';
		if (!currency) {
			errors.currency = 'Currency is required';
		} else if (!/^[A-Z]{3}$/.test(currency)) {
			errors.currency = 'Currency must be a valid 3-letter code';
		}

		// Validate due date
		const dueDate = typeof dueDateRaw === 'string' ? dueDateRaw.trim() : '';
		if (!dueDate) {
			errors.dueDate = 'Due date is required';
		} else {
			const date = new Date(dueDate);
			if (isNaN(date.getTime())) {
				errors.dueDate = 'Please enter a valid date';
			} else {
				const today = new Date();
				today.setHours(0, 0, 0, 0);
				if (date < today) {
					errors.dueDate = 'Due date cannot be in the past';
				}
			}
		}

		// Validate cashier ID
		const cashierId = typeof cashierIdRaw === 'string' ? cashierIdRaw.trim() : '';
		if (!cashierId) {
			errors.cashierId = 'Cashier selection is required';
		}

		// Return validation errors if any
		if (Object.keys(errors).length > 0) {
			return fail(400, {
				success: false,
				errors,
				values: { name, amount, currency, dueDate, cashierId }
			});
		}

		try {
			const command = new CreateInvoiceCommand({
				name,
				amount,
				currency,
				dueDate,
				cashierId
			});

			const createdInvoice = await command.execute();

			throw redirect(303, `/invoices/${createdInvoice.invoiceId}`);
		} catch (err: unknown) {
			if (isRedirect(err)) {
				throw err;
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
