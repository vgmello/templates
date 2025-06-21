import type { Cashier } from '../../app';

class CashierStore {
	// Public reactive state using class properties
	cashiers = $state<Cashier[]>([]);
	selectedCashier = $state<Cashier | null>(null);
	loading = $state<boolean>(false);
	error = $state.raw<string | null>(null); // Use $state.raw for non-reactive error messages
	searchTerm = $state<string>('');
	currencyFilter = $state<string>('all');

	// Computed properties using $derived runes - Svelte 5 best practice
	filteredCashiers = $derived.by(() => {
		let filtered = this.cashiers;
		
		// Filter by search term
		if (this.searchTerm.trim()) {
			const term = this.searchTerm.toLowerCase();
			filtered = filtered.filter(cashier => 
				cashier.name.toLowerCase().includes(term) ||
				cashier.email.toLowerCase().includes(term)
			);
		}
		
		// Filter by currency
		if (this.currencyFilter !== 'all') {
			filtered = filtered.filter(cashier => 
				cashier.cashierPayments?.some(payment => 
					payment.currency === this.currencyFilter
				)
			);
		}
		
		return filtered;
	});

	totalCashiers = $derived(this.cashiers.length);

	configuredCashiers = $derived(
		this.cashiers.filter(cashier => 
			cashier.cashierPayments && cashier.cashierPayments.length > 0
		).length
	);

	availableCurrencies = $derived.by(() => {
		const currencies = new Set<string>();
		this.cashiers.forEach(cashier => {
			cashier.cashierPayments?.forEach(payment => {
				currencies.add(payment.currency);
			});
		});
		return Array.from(currencies).sort();
	});

	// Actions for initializing from SSR data
	initializeCashiers(cashiers: Cashier[]): void {
		this.cashiers = $state.snapshot(cashiers || []);
		this.loading = false;
		this.error = null;
	}

	initializeSelectedCashier(cashier: Cashier | null): void {
		this.selectedCashier = cashier;
		
		// Add to cashiers list if not already there
		if (cashier) {
			const currentCashiers = $state.snapshot(this.cashiers);
			if (!currentCashiers.find(c => c.cashierId === cashier.cashierId)) {
				this.cashiers = [...currentCashiers, cashier];
			}
		}
	}

	// Remove server-side operations - these are handled by SvelteKit form actions
	// Store now focuses on client-side state management only
	
	// Method to update store after successful server operations
	addCashier(newCashier: Cashier): void {
		const currentCashiers = $state.snapshot(this.cashiers);
		this.cashiers = [...currentCashiers, newCashier];
	}

	updateCashierInStore(updatedCashier: Cashier): void {
		const currentCashiers = $state.snapshot(this.cashiers);
		const index = currentCashiers.findIndex(c => c.cashierId === updatedCashier.cashierId);
		if (index !== -1) {
			currentCashiers[index] = updatedCashier;
			this.cashiers = [...currentCashiers];
		}
		
		// Update selected cashier if it's the same one
		if (this.selectedCashier?.cashierId === updatedCashier.cashierId) {
			this.selectedCashier = updatedCashier;
		}
	}

	removeCashier(cashierId: string): void {
		// Remove from the list using snapshot to avoid reactivity cycles
		const currentCashiers = $state.snapshot(this.cashiers);
		this.cashiers = currentCashiers.filter(c => c.cashierId !== cashierId);
		
		// Clear selected cashier if it's the same one
		if (this.selectedCashier?.cashierId === cashierId) {
			this.selectedCashier = null;
		}
	}

	// Filter actions - now using direct property assignment
	setSearchTerm(term: string): void {
		this.searchTerm = term;
	}

	setCurrencyFilter(currency: string): void {
		this.currencyFilter = currency;
	}

	clearFilters(): void {
		this.searchTerm = '';
		this.currencyFilter = 'all';
	}

	// Utility actions
	clearError(): void {
		this.error = null;
	}

	clearSelectedCashier(): void {
		this.selectedCashier = null;
	}

	// Get cashier by ID from current list using snapshot
	getCashierById(id: string): Cashier | null {
		const cashiers = $state.snapshot(this.cashiers);
		return cashiers.find(c => c.cashierId === id) || null;
	}
}

// Create and export the singleton store instance
export const cashierStore = new CashierStore();