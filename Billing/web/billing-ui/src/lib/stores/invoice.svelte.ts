import type { Invoice } from '../../app';

/**
 * Invoice store using Svelte 5 runes
 * Client-side state management for invoices
 */
class InvoiceStore {
	// Public reactive state using class properties
	invoices = $state<Invoice[]>([]);
	selectedInvoice = $state<Invoice | null>(null);
	searchTerm = $state<string>('');
	statusFilter = $state<string>('');
	error = $state.raw<string | null>(null); // Use $state.raw for non-reactive error messages

	// Computed properties using $derived runes - Svelte 5 best practice
	filteredInvoices = $derived.by(() => {
		return this.invoices.filter(invoice => {
			const matchesSearch = invoice.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
				invoice.invoiceId.toLowerCase().includes(this.searchTerm.toLowerCase());
			
			const matchesStatus = !this.statusFilter || invoice.status === this.statusFilter;
			
			return matchesSearch && matchesStatus;
		});
	});

	// Statistics - using $derived for proper reactivity
	totalInvoices = $derived(this.invoices.length);

	totalAmount = $derived(
		this.invoices.reduce((sum, invoice) => sum + invoice.amount, 0)
	);

	paidInvoices = $derived(
		this.invoices.filter(i => i.status === 'Paid').length
	);

	draftInvoices = $derived(
		this.invoices.filter(i => i.status === 'Draft').length
	);

	cancelledInvoices = $derived(
		this.invoices.filter(i => i.status === 'Cancelled').length
	);

	// Available statuses derived from data
	availableStatuses = $derived.by(() => {
		const statuses = new Set(this.invoices.map(i => i.status));
		return Array.from(statuses).sort();
	});

	// Available currencies derived from data
	availableCurrencies = $derived.by(() => {
		const currencies = new Set(this.invoices.map(i => i.currency).filter(Boolean));
		return Array.from(currencies).sort();
	});

	// Actions for initializing from SSR data
	initializeInvoices(invoices: Invoice[]): void {
		this.invoices = $state.snapshot(invoices || []);
		this.error = null;
	}

	initializeSelectedInvoice(invoice: Invoice | null): void {
		this.selectedInvoice = invoice;
		
		// Add to invoices list if not already there
		if (invoice) {
			const currentInvoices = $state.snapshot(this.invoices);
			if (!currentInvoices.find(i => i.invoiceId === invoice.invoiceId)) {
				this.invoices = [...currentInvoices, invoice];
			}
		}
	}

	// Filter actions - using direct property assignment
	setSearchTerm(term: string): void {
		this.searchTerm = term;
	}

	setStatusFilter(status: string): void {
		this.statusFilter = status;
	}

	setError(message: string | null): void {
		this.error = message;
	}

	clearError(): void {
		this.error = null;
	}

	// Method to update store after successful server operations
	addInvoice(invoice: Invoice): void {
		const currentInvoices = $state.snapshot(this.invoices);
		this.invoices = [...currentInvoices, invoice];
	}

	updateInvoiceInStore(updatedInvoice: Invoice): void {
		const currentInvoices = $state.snapshot(this.invoices);
		const index = currentInvoices.findIndex(i => i.invoiceId === updatedInvoice.invoiceId);
		if (index !== -1) {
			currentInvoices[index] = updatedInvoice;
			this.invoices = [...currentInvoices];
		}
		
		// Update selected invoice if it's the same one
		if (this.selectedInvoice?.invoiceId === updatedInvoice.invoiceId) {
			this.selectedInvoice = updatedInvoice;
		}
	}

	removeInvoice(invoiceId: string): void {
		// Remove from the list using snapshot to avoid reactivity cycles
		const currentInvoices = $state.snapshot(this.invoices);
		this.invoices = currentInvoices.filter(i => i.invoiceId !== invoiceId);
		
		// Clear selected invoice if it's the same one
		if (this.selectedInvoice?.invoiceId === invoiceId) {
			this.selectedInvoice = null;
		}
	}

	// Utility actions
	clearFilters(): void {
		this.searchTerm = '';
		this.statusFilter = '';
	}

	clearSelectedInvoice(): void {
		this.selectedInvoice = null;
	}

	// Get invoice by ID from current list using snapshot for consistency
	getInvoiceById(invoiceId: string): Invoice | null {
		const invoices = $state.snapshot(this.invoices);
		return invoices.find(invoice => invoice.invoiceId === invoiceId) || null;
	}

	// Status-specific filters using derived computations
	getInvoicesByStatus(status: string): Invoice[] {
		const invoices = $state.snapshot(this.invoices);
		return invoices.filter(invoice => invoice.status === status);
	}

	// Due date helpers - using snapshot for safe operations
	getOverdueInvoices(): Invoice[] {
		const now = new Date();
		const invoices = $state.snapshot(this.invoices);
		return invoices.filter(invoice => 
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
		
		const invoices = $state.snapshot(this.invoices);
		return invoices.filter(invoice => 
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