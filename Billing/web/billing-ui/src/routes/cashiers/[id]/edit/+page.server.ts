import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail } from '@sveltejs/kit';
import { cashierApi } from '$lib/api';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;

	if (!id) {
		throw error(400, {
			message: 'Cashier ID is required'
		});
	}

	try {
		const cashier = await cashierApi.getCashier(id);

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
			await cashierApi.updateCashier(id, {
				name: name.trim(),
				email: email.trim() || ''
			});

			throw redirect(303, '/cashiers');
		} catch (err) {
			if (err instanceof redirect) {
				throw err;
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
