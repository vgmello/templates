import { invoiceGrpcService } from '$lib/server/grpc-client';
import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async () => {
	try {
		const invoices = await invoiceGrpcService.getInvoices();
		
		return {
			invoices
		};
	} catch (err) {
		console.error('Failed to load invoices:', err);
		throw error(500, 'Failed to load invoices. Please check if the service is running.');
	}
};