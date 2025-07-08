import type { PageServerLoad, Actions } from './$types';
import { error, redirect, fail, isRedirect } from '@sveltejs/kit';
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
		const cashier = await query.execute();

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

		try {
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
