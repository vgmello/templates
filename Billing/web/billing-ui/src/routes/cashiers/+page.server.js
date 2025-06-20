import { cashierGrpcService } from '$lib/server/grpc-client.js';
import { error } from '@sveltejs/kit';

/** @type {import('./$types').PageServerLoad} */
export async function load() {
	try {
		const cashiers = await cashierGrpcService.getCashiers();
		return {
			cashiers
		};
	} catch (err) {
		console.error('Failed to load cashiers:', err);
		
		// For edge deployment, we want to handle this gracefully
		if (err.code === 'UNAVAILABLE') {
			// Return empty state if service is unavailable
			return {
				cashiers: [],
				serviceUnavailable: true
			};
		}
		
		throw error(500, {
			message: 'Failed to load cashiers',
			details: err.message
		});
	}
}