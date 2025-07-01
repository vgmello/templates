import type { PageServerLoad } from './$types';
import { invoiceBffService } from '$lib/server/invoice-bff-service.js';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;
	
	if (!id) {
		throw error(400, {
			message: 'Invoice ID is required'
		});
	}

	try {
		const invoice = await invoiceBffService.getInvoice(id);
		
		return {
			invoice
		};
	} catch (err: any) {
		console.error('Failed to load invoice:', err);
		
		if (err.status === 404) {
			throw error(404, {
				message: 'Invoice not found'
			});
		}
		
		throw error(500, {
			message: 'Failed to load invoice. Please try again later.'
		});
	}
};