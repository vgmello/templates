import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { serverApiClient, ApiError } from '$lib/infrastructure';
import type { InvoiceData } from '$lib/domain';

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;
	
	if (!id) {
		throw error(400, {
			message: 'Invoice ID is required'
		});
	}

	try {
		const invoice = await serverApiClient.get<InvoiceData>(`/invoices/${id}`);
		
		return {
			invoice
		};
	} catch (err) {
		console.error('Failed to load invoice:', err);
		
		if (err instanceof ApiError && err.status === 404) {
			throw error(404, {
				message: 'Invoice not found'
			});
		}
		
		throw error(500, {
			message: 'Failed to load invoice. Please try again later.'
		});
	}
};