import { error } from '@sveltejs/kit';
import { cashierService } from '$lib/api.js';

/** @type {import('./$types').PageLoad} */
export async function load({ params, fetch }) {
	try {
		// Use the API service with SvelteKit's fetch function
		const cashier = await cashierService.getCashier(params.id, fetch);
		return {
			cashier
		};
	} catch (err) {
		// If it's already a SvelteKit error, re-throw it
		if (err.status) {
			throw err;
		}
		
		// Handle API errors and map to SvelteKit errors
		if (err.status === 404) {
			throw error(404, 'Cashier not found');
		}
		
		console.error('Failed to load cashier:', err);
		throw error(500, 'Failed to load cashier data');
	}
}