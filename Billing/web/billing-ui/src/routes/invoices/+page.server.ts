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
		
		// Return mock data for testing/fallback
		const mockInvoices = [
			{
				invoiceId: "mock-invoice-1",
				name: "Mock Invoice 1",
				status: "Draft",
				amount: 100.00,
				currency: "USD",
				dueDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
				cashierId: "mock-cashier-1",
				createdDateUtc: new Date().toISOString(),
				updatedDateUtc: new Date().toISOString(),
				version: 1
			},
			{
				invoiceId: "mock-invoice-2",
				name: "Mock Invoice 2",
				status: "Paid",
				amount: 250.50,
				currency: "USD",
				dueDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
				createdDateUtc: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString(),
				updatedDateUtc: new Date().toISOString(),
				version: 2
			}
		];
		
		return {
			invoices: mockInvoices,
			error: 'Using mock data - service unavailable.'
		};
	}
};