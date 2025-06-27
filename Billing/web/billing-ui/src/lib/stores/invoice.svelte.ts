import type { Invoice } from '../../app';

/**
 * Invoice store using Svelte 5 runes
 * Client-side state management for invoices
 */
class InvoiceStore {
	invoices = $state<Invoice[]>([]);
	selectedInvoice = $state<Invoice | null>(null);
	searchTerm = $state('');
	statusFilter = $state('');
	errorMessage = $state<string | null>(null);

	// Computed properties using $derived
	filteredInvoices = $derived(() => {
		return this.invoices.filter(invoice => {
			const matchesSearch = invoice.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
				invoice.invoiceId.toLowerCase().includes(this.searchTerm.toLowerCase());
			
			const matchesStatus = !this.statusFilter || invoice.status === this.statusFilter;
			
			return matchesSearch && matchesStatus;
		});
	});

	// Statistics
	totalInvoices = $derived(() => this.invoices.length);
	totalAmount = $derived(() => this.invoices.reduce((sum, invoice) => sum + invoice.amount, 0));
	paidInvoices = $derived(() => this.invoices.filter(i => i.status === 'Paid').length);
	draftInvoices = $derived(() => this.invoices.filter(i => i.status === 'Draft').length);
	cancelledInvoices = $derived(() => this.invoices.filter(i => i.status === 'Cancelled').length);

	// Available statuses derived from data
	availableStatuses = $derived(() => {
		const statuses = new Set(this.invoices.map(i => i.status));
		return Array.from(statuses).sort();
	});

	// Available currencies derived from data
	availableCurrencies = $derived(() => {
		const currencies = new Set(this.invoices.map(i => i.currency).filter(Boolean));
		return Array.from(currencies).sort();
	});

	// Methods
	initializeInvoices(invoices: Invoice[]) {
		this.invoices = invoices;
		this.errorMessage = null;
	}

	initializeSelectedInvoice(invoice: Invoice | null) {
		this.selectedInvoice = invoice;
	}

	setSearchTerm(term: string) {
		this.searchTerm = term;
	}

	setStatusFilter(status: string) {
		this.statusFilter = status;
	}

	setError(message: string | null) {
		this.errorMessage = message;
	}

	clearError() {
		this.errorMessage = null;
	}

	addInvoice(invoice: Invoice) {
		this.invoices = [...this.invoices, invoice];
	}

	updateInvoice(updatedInvoice: Invoice) {
		this.invoices = this.invoices.map(invoice => 
			invoice.invoiceId === updatedInvoice.invoiceId ? updatedInvoice : invoice
		);
		
		// Update selected invoice if it's the one being updated
		if (this.selectedInvoice?.invoiceId === updatedInvoice.invoiceId) {
			this.selectedInvoice = updatedInvoice;
		}
	}

	removeInvoice(invoiceId: string) {
		this.invoices = this.invoices.filter(invoice => invoice.invoiceId !== invoiceId);
		
		// Clear selected invoice if it's the one being removed
		if (this.selectedInvoice?.invoiceId === invoiceId) {
			this.selectedInvoice = null;
		}
	}

	getInvoiceById(invoiceId: string): Invoice | undefined {
		return this.invoices.find(invoice => invoice.invoiceId === invoiceId);
	}

	// Status-specific filters
	getInvoicesByStatus(status: string): Invoice[] {
		return this.invoices.filter(invoice => invoice.status === status);
	}

	// Due date helpers
	getOverdueInvoices(): Invoice[] {
		const now = new Date();
		return this.invoices.filter(invoice => 
			invoice.dueDate && 
			new Date(invoice.dueDate) < now && 
			invoice.status !== 'Paid' && 
			invoice.status !== 'Cancelled'
		);
	}

	getDueSoonInvoices(days = 7): Invoice[] {
		const now = new Date();
		const futureDate = new Date();
		futureDate.setDate(now.getDate() + days);
		
		return this.invoices.filter(invoice => 
			invoice.dueDate && 
			new Date(invoice.dueDate) >= now &&
			new Date(invoice.dueDate) <= futureDate &&
			invoice.status !== 'Paid' && 
			invoice.status !== 'Cancelled'
		);
	}
}

// Export singleton instance
export const invoiceStore = new InvoiceStore();