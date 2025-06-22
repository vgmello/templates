import { cashierGrpcService } from '$lib/server/grpc-client';
import { fail, redirect } from '@sveltejs/kit';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ params }) => {
	try {
		const cashier = await cashierGrpcService.getCashier(params.id);
		return {
			cashier
		};
	} catch (error) {
		console.error('Failed to load cashier:', error);
		throw redirect(303, '/cashiers');
	}
};

export const actions: Actions = {
	update: async ({ request, params }) => {
		const data = await request.formData();
		const name = data.get('name');
		const email = data.get('email');
		const currencies = data.getAll('currencies');

		// Validation
		if (!name || !email || currencies.length === 0) {
			return fail(400, {
				name,
				email,
				currencies,
				missing: true,
				message: 'Please fill in all required fields and add at least one currency.'
			});
		}

		try {
			const cashierData = {
				name: name.toString(),
				email: email.toString(),
				currencies: currencies.map(c => c.toString())
			};

			const updatedCashier = await cashierGrpcService.updateCashier(params.id, cashierData);
			
			// Redirect to the updated cashier's detail page
			throw redirect(303, `/cashiers/${updatedCashier.cashierId}`);
		} catch (err) {
			console.error('Failed to update cashier:', err);
			
			if ((err as any).code === 'NOT_FOUND') {
				return fail(404, {
					name,
					email,
					currencies,
					notFound: true,
					message: 'Cashier not found.'
				});
			}
			
			return fail(500, {
				name,
				email,
				currencies,
				error: true,
				message: (err as Error).message || 'Failed to update cashier. Please try again.'
			});
		}
	}
};