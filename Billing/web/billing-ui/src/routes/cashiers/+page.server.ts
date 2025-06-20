import { cashierGrpcService } from '$lib/server/grpc-client';
import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async () => {
	try {
		const cashiers = await cashierGrpcService.getCashiers();
		return {
			cashiers
		};
	} catch (err) {
		console.error('Failed to load cashiers:', err);
		
		// For edge deployment, we want to handle this gracefully
		if ((err as any).code === 'UNAVAILABLE') {
			// Return empty state if service is unavailable
			return {
				cashiers: [],
				serviceUnavailable: true
			};
		}
		
		throw error(500, `Failed to load cashiers: ${(err as Error).message}`);
	}
}