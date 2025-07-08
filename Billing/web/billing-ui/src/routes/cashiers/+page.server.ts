import type { PageServerLoad, Actions } from './$types';
import { error, fail } from '@sveltejs/kit';
import { GetCashiersQuery } from '$lib/cashiers/actions/GetCashiersQuery';
import { DeleteCashierCommand } from '$lib/cashiers/actions/DeleteCashierCommand';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async ({ url, depends }) => {
	depends('cashiers:list');
	
	try {
		// Extract query parameters for potential filtering
		const page = url.searchParams.get('page');
		const pageSize = url.searchParams.get('pageSize');
		const search = url.searchParams.get('search');
		const sortBy = url.searchParams.get('sortBy');
		const sortDescending = url.searchParams.get('sortDescending') === 'true';

		const query = new GetCashiersQuery({
			...(page && { page: parseInt(page) }),
			...(pageSize && { pageSize: parseInt(pageSize) }),
			...(search && { search }),
			...(sortBy && { sortBy }),
			...(sortDescending !== undefined && { sortDescending })
		});

		const cashiers = await query.execute();

		return {
			cashiers
		};
	} catch (err) {
		console.error('Failed to load cashiers:', err);
		throw error(500, {
			message: 'Failed to load cashiers. Please try again later.'
		});
	}
};

export const actions: Actions = {
	delete: async ({ request }) => {
		const data = await request.formData();
		const id = data.get('id') as string;

		if (!id) {
			return fail(400, {
				success: false,
				errors: ['Cashier ID is required']
			});
		}

		try {
			const command = new DeleteCashierCommand(id);
			await command.execute();
			return { success: true };
		} catch (err) {
			console.error('Failed to delete cashier:', err);

			if (err instanceof ApiError && err.status === 404) {
				return fail(404, {
					success: false,
					errors: ['Cashier not found']
				});
			}

			return fail(500, {
				success: false,
				errors: ['Failed to delete cashier. Please try again later.']
			});
		}
	}
};
