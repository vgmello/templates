import type { Actions } from './$types';
import { redirect, fail } from '@sveltejs/kit';
import { CashierService, ValidationError } from '$lib/cashiers';
import { ApiError } from '$lib/infrastructure';

export const actions: Actions = {
	default: async ({ request }) => {
		const data = await request.formData();
		const cashierService = new CashierService();

		const name = data.get('name') as string;
		const email = data.get('email') as string;

		try {
			await cashierService.createCashier({
				name: name || '',
				email: email || ''
			});

			throw redirect(303, '/cashiers');
		} catch (err) {
			// If it's a redirect, just re-throw it
			if (
				err &&
				typeof err === 'object' &&
				'status' in err &&
				typeof err.status === 'number' &&
				err.status >= 300 &&
				err.status < 400
			) {
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

			console.error('Failed to create cashier:', err);

			if (err instanceof ApiError && err.status === 400) {
				return fail(400, {
					success: false,
					errors: { form: err.message },
					values: { name, email }
				});
			}

			return fail(500, {
				success: false,
				errors: { form: 'Failed to create cashier. Please try again later.' },
				values: { name, email }
			});
		}
	}
};
