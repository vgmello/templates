import { cashierGrpcService } from '$lib/server/grpc-client';
import { error, redirect } from '@sveltejs/kit';
import type { PageServerLoad, Actions } from './$types';

export const load: PageServerLoad = async ({ params }) => {
	try {
		const cashier = await cashierGrpcService.getCashier(params.id);
		return {
			cashier
		};
	} catch (err) {
		console.error('Failed to load cashier:', err);
		
		// Handle specific gRPC errors
		if ((err as any).code === 'NOT_FOUND') {
			throw error(404, `Cashier not found: No cashier found with ID: ${params.id}`);
		}
		
		if ((err as any).code === 'UNAVAILABLE') {
			throw error(503, 'Service temporarily unavailable: The billing service is currently unavailable. Please try again later.');
		}
		
		throw error(500, `Failed to load cashier: ${(err as Error).message}`);
	}
}

export const actions: Actions = {
	delete: async ({ params }) => {
		try {
			await cashierGrpcService.deleteCashier(params.id);
			throw redirect(303, '/cashiers');
		} catch (err) {
			console.error('Failed to delete cashier:', err);
			
			if ((err as any).code === 'NOT_FOUND') {
				throw error(404, `Cashier not found: No cashier found with ID: ${params.id}`);
			}
			
			throw error(500, `Failed to delete cashier: ${(err as Error).message}`);
		}
	}
};