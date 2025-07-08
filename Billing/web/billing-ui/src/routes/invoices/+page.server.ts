import type { PageServerLoad, Actions } from './$types';
import { error, fail } from '@sveltejs/kit';
import { GetInvoicesQuery } from '$lib/invoices/actions/GetInvoicesQuery';
import { CancelInvoiceCommand } from '$lib/invoices/actions/CancelInvoiceCommand';
import { ApiError } from '$lib/infrastructure';

export const load: PageServerLoad = async ({ url }) => {
	try {
		// Extract query parameters
		const status = url.searchParams.get('status') || undefined;
		const cashierId = url.searchParams.get('cashierId') || undefined;
		const fromDate = url.searchParams.get('fromDate') || undefined;
		const toDate = url.searchParams.get('toDate') || undefined;
		const skip = url.searchParams.get('skip')
			? parseInt(url.searchParams.get('skip')!)
			: undefined;
		const take = url.searchParams.get('take')
			? parseInt(url.searchParams.get('take')!)
			: undefined;

		// Fetch invoices using query
		const query = new GetInvoicesQuery({
			...(status && { status }),
			...(cashierId && { cashierId }),
			...(fromDate && { fromDate }),
			...(toDate && { toDate }),
			...(skip !== undefined && { skip }),
			...(take !== undefined && { take })
		});
		
		const invoices = await query.execute();

		// Temporary simple summary without domain service
		const summary = {
			totalInvoices: invoices.length,
			currencySummaries: []
		};

		return {
			invoices,
			summary
		};
	} catch (err) {
		console.error('Failed to load invoices:', err);
		throw error(500, {
			message: 'Failed to load invoices. Please try again later.'
		});
	}
};

export const actions: Actions = {
	cancel: async ({ request }) => {
		const data = await request.formData();
		const id = data.get('id') as string;

		if (!id) {
			return fail(400, {
				success: false,
				errors: ['Invoice ID is required']
			});
		}

		try {
			const command = new CancelInvoiceCommand(id);
			await command.execute();
			return { success: true };
		} catch (err) {
			console.error('Failed to cancel invoice:', err);

			if (err instanceof ApiError && err.status === 404) {
				return fail(404, {
					success: false,
					errors: ['Invoice not found']
				});
			}

			return fail(500, {
				success: false,
				errors: ['Failed to cancel invoice. Please try again later.']
			});
		}
	}
};
