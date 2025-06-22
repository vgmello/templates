import { cashierGrpcService } from '$lib/server/grpc-client';
import { fail, redirect } from '@sveltejs/kit';
import type { Actions } from './$types';

export const actions: Actions = {
	default: async ({ request }) => {
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

			const newCashier = await cashierGrpcService.createCashier(cashierData);
			
			// Redirect to the new cashier's detail page
			throw redirect(303, `/cashiers/${newCashier.cashierId}`);
		} catch (err) {
			console.error('Failed to create cashier:', err);
			
			if ((err as any).code === 'ALREADY_EXISTS') {
				return fail(409, {
					name,
					email,
					currencies,
					duplicate: true,
					message: 'A cashier with this email already exists.'
				});
			}
			
			return fail(500, {
				name,
				email,
				currencies,
				error: true,
				message: (err as Error).message || 'Failed to create cashier. Please try again.'
			});
		}
	}
};