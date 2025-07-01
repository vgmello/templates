import type { PageServerLoad } from './$types';
import { invoiceBffService } from '$lib/server/invoice-bff-service.js';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async () => {
	try {
		const data = await invoiceBffService.getInvoiceCreateData();
		
		return {
			cashiers: data.cashiers
		};
	} catch (err) {
		console.error('Failed to load invoice create data:', err);
		throw error(500, {
			message: 'Failed to load required data. Please try again later.'
		});
	}
};