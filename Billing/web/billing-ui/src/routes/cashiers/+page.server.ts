import type { PageServerLoad, Actions } from './$types';
import { error, redirect } from '@sveltejs/kit';
import { serverApiClient, ApiError } from '$lib/infrastructure';
import { CashierService } from '$lib/domain';
import type { CashierData } from '$lib/domain';

const cashierService = new CashierService();

export const load: PageServerLoad = async ({ url }) => {
	try {
		// Extract query parameters for potential filtering
		const params: Record<string, string | number | boolean> = {};
		for (const [key, value] of url.searchParams.entries()) {
			params[key] = value;
		}

		const cashiers = await serverApiClient.get<CashierData[]>('/cashiers', params);
		
		return {
			cashiers
		};
	} catch (err) {
		console.error('Failed to load cashiers:', err);
		throw error(500, {
			message: 'Failed to load cashiers. Please try again later.'
		});
	}
};

export const actions: Actions = {
	create: async ({ request }) => {
		const data = await request.formData();
		
		const cashierData = {
			name: data.get('name') as string,
			email: data.get('email') as string,
			phone: data.get('phone') as string || '',
			isActive: data.get('isActive') === 'on',
			supportedCurrencies: data.getAll('supportedCurrencies') as string[]
		};

		// Validate using domain service
		const validationErrors = cashierService.validateCashierData(cashierData);
		if (validationErrors.length > 0) {
			return {
				success: false,
				errors: validationErrors,
				data: cashierData
			};
		}

		try {
			const createdCashier = await serverApiClient.post<CashierData>('/cashiers', cashierData);
			throw redirect(303, `/cashiers`);
		} catch (err) {
			console.error('Failed to create cashier:', err);
			
			if (err instanceof ApiError && err.status >= 400 && err.status < 500) {
				return {
					success: false,
					errors: [err.message],
					data: cashierData
				};
			}
			
			return {
				success: false,
				errors: ['Failed to create cashier. Please try again later.'],
				data: cashierData
			};
		}
	},

	delete: async ({ request }) => {
		const data = await request.formData();
		const id = data.get('id') as string;

		if (!id) {
			return {
				success: false,
				errors: ['Cashier ID is required']
			};
		}

		try {
			await serverApiClient.delete(`/cashiers/${id}`);
			return { success: true };
		} catch (err) {
			console.error('Failed to delete cashier:', err);
			
			if (err instanceof ApiError && err.status === 404) {
				return {
					success: false,
					errors: ['Cashier not found']
				};
			}
			
			return {
				success: false,
				errors: ['Failed to delete cashier. Please try again later.']
			};
		}
	}
};