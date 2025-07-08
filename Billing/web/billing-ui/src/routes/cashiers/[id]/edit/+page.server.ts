import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail, isRedirect } from '@sveltejs/kit';
import { ValidationError } from '$lib/cashiers/validators/CashierValidator';
import { Cashier } from '$lib/cashiers/models/Cashier';
import { ApiError } from '$lib/infrastructure';
import { GetCashierQuery } from '$lib/cashiers/actions/GetCashierQuery';
import { UpdateCashierCommand } from '$lib/cashiers/actions/UpdateCashierCommand';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;

	if (!id) {
		throw error(400, {
			message: 'Cashier ID is required'
		});
	}

	try {
		const query = new GetCashierQuery(id);

		const cashierDTO = await query.execute();

		const cashier = new Cashier({
			id: cashierDTO.cashierId,
			name: cashierDTO.name,
			email: cashierDTO.email,
			phone: '', // API doesn't currently return phone
			isActive: true, // API doesn't currently return status
			supportedCurrencies: ['USD'], // Default for now
			createdAt: new Date().toISOString(),
			updatedAt: new Date().toISOString()
		});

		return {
			cashier
		};
	} catch (err) {
		console.error('Failed to load cashier:', err);

		if (err instanceof ApiError && err.status === 404) {
			throw error(404, {
				message: 'Cashier not found'
			});
		}

		throw error(500, {
			message: 'Failed to load cashier. Please try again later.'
		});
	}
};

export const actions: Actions = {
	default: async ({ params, request }) => {
		const { id } = params;

		if (!id) {
			return fail(400, {
				success: false,
				errors: { form: 'Cashier ID is required' },
				values: {}
			});
		}

		const data = await request.formData();

		const name = data.get('name') as string;
		const email = data.get('email') as string;
		const phone = data.get('phone') as string;
		const supportedCurrencies = data.get('supportedCurrencies') as string;
		const isActive = data.get('isActive') as string;

		try {
			// For now, just update with name and email until backend supports additional fields
			const command = new UpdateCashierCommand(id, {
				name: name || '',
				email: email || ''
			});

			await command.execute();

			throw redirect(303, '/cashiers');
		} catch (err) {
			if (isRedirect(err)) {
				throw err;
			}

			// Handle validation errors from service
			if (err instanceof ValidationError) {
				return fail(400, {
					success: false,
					errors: err.errors,
					values: { name, email }
				});
			}

			console.error('Failed to update cashier:', err);

			if (err instanceof ApiError && err.status === 404) {
				return fail(404, {
					success: false,
					errors: { form: 'Cashier not found' },
					values: { name, email }
				});
			}

			if (err instanceof ApiError && err.status === 400) {
				return fail(400, {
					success: false,
					errors: { form: err.message },
					values: { name, email }
				});
			}

			return fail(500, {
				success: false,
				errors: { form: 'Failed to update cashier. Please try again later.' },
				values: { name, email }
			});
		}
	}
};
