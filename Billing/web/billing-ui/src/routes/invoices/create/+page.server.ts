import { invoiceGrpcService, cashierGrpcService } from '$lib/server/grpc-client';
import { fail, redirect } from '@sveltejs/kit';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async () => {
	try {
		// Load cashiers for the dropdown
		const cashiers = await cashierGrpcService.getCashiers();
		
		return {
			cashiers
		};
	} catch (err) {
		console.error('Failed to load cashiers:', err);
		return {
			cashiers: []
		};
	}
};

export const actions: Actions = {
	default: async ({ request }) => {
		const data = await request.formData();
		
		const name = data.get('name') as string;
		const amount = parseFloat(data.get('amount') as string);
		const currency = data.get('currency') as string;
		const dueDate = data.get('dueDate') as string;
		const cashierId = data.get('cashierId') as string;

		// Validation
		if (!name || name.trim().length < 2) {
			return fail(400, {
				error: 'Invoice name must be at least 2 characters long',
				name,
				amount: amount?.toString() || '',
				currency,
				dueDate,
				cashierId
			});
		}

		if (!amount || amount <= 0) {
			return fail(400, {
				error: 'Amount must be greater than 0',
				name,
				amount: amount?.toString() || '',
				currency,
				dueDate,
				cashierId
			});
		}

		try {
			const invoiceData = {
				name: name.trim(),
				amount,
				currency: currency || 'USD',
				dueDate: dueDate || undefined,
				cashierId: cashierId || undefined
			};

			const invoice = await invoiceGrpcService.createInvoice(invoiceData);
			
			throw redirect(303, `/invoices/${invoice.invoiceId}`);
		} catch (err: any) {
			console.error('Failed to create invoice:', err);
			
			let errorMessage = 'Failed to create invoice';
			if (err.message?.includes('validation')) {
				errorMessage = 'Invalid invoice data';
			} else if (err.message?.includes('cashier')) {
				errorMessage = 'Selected cashier not found';
			}
			
			return fail(500, {
				error: errorMessage,
				name,
				amount: amount?.toString() || '',
				currency,
				dueDate,
				cashierId
			});
		}
	}
};