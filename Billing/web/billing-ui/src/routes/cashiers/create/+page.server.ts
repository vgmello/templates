import type { Actions } from './$types';
import { redirect, fail, isRedirect } from '@sveltejs/kit';
import { CreateCashierCommand } from '$lib/cashiers/actions/CreateCashierCommand';
import { ApiError } from '$lib/infrastructure';

export const actions: Actions = {
	default: async ({ request }) => {
		const data = await request.formData();

		const name = data.get('name') as string;
		const email = data.get('email') as string;
		const phone = data.get('phone') as string;
		const supportedCurrencies = data.get('supportedCurrencies') as string;
		const isActive = data.get('isActive') as string;

		try {
			const createCashierCommand = new CreateCashierCommand({
				name: name || '',
				email: email || ''
			});

			await createCashierCommand.execute();

			throw redirect(303, '/cashiers');
		} catch (err) {
			if (isRedirect(err)) {
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
