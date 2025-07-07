import type { PageServerLoad, Actions } from './$types';
import { error, redirect } from '@sveltejs/kit';
import { serverApiClient, ApiError } from '$lib/infrastructure';
import { InvoiceService } from '$lib/domain';
import type { CashierData, InvoiceData } from '$lib/domain';

const invoiceService = new InvoiceService();

export const load: PageServerLoad = async () => {
	try {
		const cashiers = await serverApiClient.get<CashierData[]>('/cashiers');
		
		return {
			cashiers
		};
	} catch (err) {
		console.error('Failed to load invoice create data:', err);
		throw error(500, {
			message: 'Failed to load required data. Please try again later.'
		});
	}
};

export const actions: Actions = {
	create: async ({ request }) => {
		const data = await request.formData();
		
		const invoiceData = {
			number: invoiceService.generateInvoiceNumber(),
			customerName: data.get('customerName') as string,
			customerEmail: data.get('customerEmail') as string,
			description: data.get('description') as string,
			amount: parseInt(data.get('amount') as string),
			currency: data.get('currency') as string,
			cashierId: data.get('cashierId') as string || null,
			issueDate: new Date().toISOString(),
			dueDate: data.get('dueDate') as string,
			status: 'draft' as const
		};

		// Validate using domain service
		const validationErrors = invoiceService.validateInvoiceData(invoiceData);
		if (validationErrors.length > 0) {
			return {
				success: false,
				errors: validationErrors,
				data: invoiceData
			};
		}

		try {
			const createdInvoice = await serverApiClient.post<InvoiceData>('/invoices', invoiceData);
			throw redirect(303, `/invoices/${createdInvoice.id}`);
		} catch (err) {
			console.error('Failed to create invoice:', err);
			
			if (err instanceof ApiError && err.status >= 400 && err.status < 500) {
				return {
					success: false,
					errors: [err.message],
					data: invoiceData
				};
			}
			
			return {
				success: false,
				errors: ['Failed to create invoice. Please try again later.'],
				data: invoiceData
			};
		}
	}
};