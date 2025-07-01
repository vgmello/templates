import type { PageServerLoad } from './$types';
import { invoiceBffService } from '$lib/server/invoice-bff-service.js';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async () => {
	try {
		const data = await invoiceBffService.getInvoicesWithSummary();
		
		return {
			invoices: data.invoices,
			summary: data.summary
		};
	} catch (err) {
		console.error('Failed to load invoices:', err);
		throw error(500, {
			message: 'Failed to load invoices. Please try again later.'
		});
	}
};