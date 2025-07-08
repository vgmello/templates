import type { PageServerLoad } from './$types';
import { cashierBffService } from '$lib/server/cashier-bff-service.js';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async ({ locals }) => {
	try {
		const cashiers = await cashierBffService.getCashiers({}, locals.traceContext);
		
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