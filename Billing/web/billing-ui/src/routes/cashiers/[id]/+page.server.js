import { cashierGrpcService } from '$lib/server/grpc-client.js';
import { error, redirect } from '@sveltejs/kit';

/** @type {import('./$types').PageServerLoad} */
export async function load({ params }) {
	try {
		const cashier = await cashierGrpcService.getCashier(params.id);
		return {
			cashier
		};
	} catch (err) {
		console.error('Failed to load cashier:', err);
		
		// Handle specific gRPC errors
		if (err.code === 'NOT_FOUND') {
			throw error(404, {
				message: 'Cashier not found',
				details: `No cashier found with ID: ${params.id}`
			});
		}
		
		if (err.code === 'UNAVAILABLE') {
			throw error(503, {
				message: 'Service temporarily unavailable',
				details: 'The billing service is currently unavailable. Please try again later.'
			});
		}
		
		throw error(500, {
			message: 'Failed to load cashier',
			details: err.message
		});
	}
}

/** @type {import('./$types').Actions} */
export const actions = {
	delete: async ({ params }) => {
		try {
			await cashierGrpcService.deleteCashier(params.id);
			throw redirect(303, '/cashiers');
		} catch (err) {
			console.error('Failed to delete cashier:', err);
			
			if (err.code === 'NOT_FOUND') {
				throw error(404, {
					message: 'Cashier not found',
					details: `No cashier found with ID: ${params.id}`
				});
			}
			
			throw error(500, {
				message: 'Failed to delete cashier',
				details: err.message
			});
		}
	}
};