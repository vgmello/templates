import type { PageServerLoad } from './$types';
import { error } from '@sveltejs/kit';
import { serverApiClient } from '$lib/infrastructure';
import { InvoiceService } from '$lib/domain';
import type { InvoiceData } from '$lib/domain';

const invoiceService = new InvoiceService();

export const load: PageServerLoad = async ({ url }) => {
	try {
		// Extract query parameters
		const status = url.searchParams.get('status') || undefined;
		const cashierId = url.searchParams.get('cashierId') || undefined;
		const fromDate = url.searchParams.get('fromDate') || undefined;
		const toDate = url.searchParams.get('toDate') || undefined;
		const skip = url.searchParams.get('skip') ? parseInt(url.searchParams.get('skip')!) : undefined;
		const take = url.searchParams.get('take') ? parseInt(url.searchParams.get('take')!) : undefined;

		// Build query parameters
		const params: Record<string, string | number> = {};
		if (status) params.status = status;
		if (cashierId) params.cashierId = cashierId;
		if (fromDate) params.fromDate = fromDate;
		if (toDate) params.toDate = toDate;
		if (skip !== undefined) params.skip = skip;
		if (take !== undefined) params.take = take;

		// Fetch invoices directly from API
		const invoices = await serverApiClient.get<InvoiceData[]>('/invoices', params);
		
		// Calculate summary using domain service
		const summary = invoiceService.calculateSummary(invoices);
		
		return {
			invoices,
			summary
		};
	} catch (err) {
		console.error('Failed to load invoices:', err);
		throw error(500, {
			message: 'Failed to load invoices. Please try again later.'
		});
	}
};