import type { PageServerLoad } from './$types';
import { cashierBffService } from '$lib/server/cashier-bff-service.js';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;
	
	if (!id) {
		throw error(400, {
			message: 'Cashier ID is required'
		});
	}

	try {
		const cashier = await cashierBffService.getCashier(id);
		
		return {
			cashier
		};
	} catch (err: any) {
		console.error('Failed to load cashier:', err);
		
		if (err.status === 404) {
			throw error(404, {
				message: 'Cashier not found'
			});
		}
		
		throw error(500, {
			message: 'Failed to load cashier. Please try again later.'
		});
	}
};