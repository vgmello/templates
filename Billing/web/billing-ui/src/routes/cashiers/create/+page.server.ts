import type { Actions } from './$types';
import { redirect, fail } from '@sveltejs/kit';
import { cashierApi } from '$lib/cashiers';
import { ApiError } from '$lib/infrastructure';

export const actions: Actions = {
	default: async ({ request }) => {
		const data = await request.formData();

		const name = data.get('name') as string;
		const email = data.get('email') as string;

		// Server-side validation
		const errors: Record<string, string> = {};

		if (!name?.trim()) {
			errors.name = 'Name is required';
		} else if (name.trim().length < 2) {
			errors.name = 'Name must be at least 2 characters';
		} else if (name.trim().length > 100) {
			errors.name = 'Name must not exceed 100 characters';
		}

		if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
			errors.email = 'Please enter a valid email address';
		}

		if (Object.keys(errors).length > 0) {
			return fail(400, {
				success: false,
				errors,
				values: { name, email }
			});
		}

		try {
			await cashierApi.createCashier({
				name: name.trim(),
				email: email.trim() || ''
			});

			throw redirect(303, '/cashiers');
		} catch (err) {
			// If it's a redirect, just re-throw it
			if (
				err &&
				typeof err === 'object' &&
				'status' in err &&
				err.status >= 300 &&
				err.status < 400
			) {
				throw err;
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
