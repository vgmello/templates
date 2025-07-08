import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail } from '@sveltejs/kit';
import { CashierService, ValidationError } from '$lib/cashiers';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;
	const cashierService = new CashierService();

	if (!id) {
		throw error(400, {
			message: 'Cashier ID is required'
		});
	}

	try {
		const cashier = await cashierService.getCashier(id);

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
		const cashierService = new CashierService();

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

		try {
			await cashierService.updateCashier(id, {
				name: name || '',
				email: email || ''
			});

			throw redirect(303, '/cashiers');
		} catch (err) {
			if (err instanceof redirect) {
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
