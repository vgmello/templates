import type { PageServerLoad, Actions } from './$types';
import { error, redirect } from '@sveltejs/kit';
import { serverApiClient, ApiError } from '$lib/infrastructure';
import { CashierService } from '$lib/domain';
import type { CashierData } from '$lib/domain';

const cashierService = new CashierService();

export const load: PageServerLoad = async ({ params }) => {
	const { id } = params;
	
	if (!id) {
		throw error(400, {
			message: 'Cashier ID is required'
		});
	}

	try {
		const cashier = await serverApiClient.get<CashierData>(`/cashiers/${id}`);
		
		return {
			cashier
		};
	} catch (err) {
		console.error('Failed to load cashier:', err);
		
		if (err instanceof ApiError && err.status === 404) {
			throw error(404, {
				message: 'Cashier not found'
			});
		}
		
		throw error(500, {
			message: 'Failed to load cashier. Please try again later.'
		});
	}
};

export const actions: Actions = {
	update: async ({ params, request }) => {
		const { id } = params;
		
		if (!id) {
			return {
				success: false,
				errors: ['Cashier ID is required']
			};
		}

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
			await serverApiClient.put<CashierData>(`/cashiers/${id}`, cashierData);
			throw redirect(303, `/cashiers`);
		} catch (err) {
			console.error('Failed to update cashier:', err);
			
			if (err instanceof ApiError && err.status === 404) {
				return {
					success: false,
					errors: ['Cashier not found']
				};
			}
			
			if (err instanceof ApiError && err.status >= 400 && err.status < 500) {
				return {
					success: false,
					errors: [err.message],
					data: cashierData
				};
			}
			
			return {
				success: false,
				errors: ['Failed to update cashier. Please try again later.'],
				data: cashierData
			};
		}
	}
};