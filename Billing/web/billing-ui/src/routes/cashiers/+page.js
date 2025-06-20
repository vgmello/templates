import { cashierStore } from '$lib/stores/cashier.svelte.js';

/** @type {import('./$types').PageLoad} */
export async function load({ fetch }) {
	// Load cashiers into the store
	await cashierStore.loadCashiers(fetch);
	
	// Return empty object since data is now in the store
	return {};
}