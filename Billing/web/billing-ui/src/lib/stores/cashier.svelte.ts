import type { Cashier, CreateCashierRequest, UpdateCashierRequest } from '../../app';

class CashierStore {
	// Private state
	#cashiers = $state<Cashier[]>([]);
	#selectedCashier = $state<Cashier | null>(null);
	#loading = $state<boolean>(false);
	#error = $state<string | null>(null);
	#searchTerm = $state<string>('');
	#currencyFilter = $state<string>('all');

	// Public getters
	get cashiers(): Cashier[] {
		return this.#cashiers;
	}

	get selectedCashier(): Cashier | null {
		return this.#selectedCashier;
	}

	get loading(): boolean {
		return this.#loading;
	}

	get error(): string | null {
		return this.#error;
	}

	get searchTerm(): string {
		return this.#searchTerm;
	}

	get currencyFilter(): string {
		return this.#currencyFilter;
	}

	// Computed properties (regular getters, computed in components)
	get filteredCashiers(): Cashier[] {
		let filtered = this.#cashiers;
		
		// Filter by search term
		if (this.#searchTerm.trim()) {
			const term = this.#searchTerm.toLowerCase();
			filtered = filtered.filter(cashier => 
				cashier.name.toLowerCase().includes(term) ||
				cashier.email.toLowerCase().includes(term)
			);
		}
		
		// Filter by currency
		if (this.#currencyFilter !== 'all') {
			filtered = filtered.filter(cashier => 
				cashier.cashierPayments?.some(payment => 
					payment.currency === this.#currencyFilter
				)
			);
		}
		
		return filtered;
	}

	get totalCashiers(): number {
		return this.#cashiers.length;
	}

	get configuredCashiers(): number {
		return this.#cashiers.filter(cashier => 
			cashier.cashierPayments && cashier.cashierPayments.length > 0
		).length;
	}

	get availableCurrencies(): string[] {
		const currencies = new Set<string>();
		this.#cashiers.forEach(cashier => {
			cashier.cashierPayments?.forEach(payment => {
				currencies.add(payment.currency);
			});
		});
		return Array.from(currencies).sort();
	}

	// Actions for initializing from SSR data
	initializeCashiers(cashiers: Cashier[]): void {
		this.#cashiers = cashiers || [];
		this.#loading = false;
		this.#error = null;
	}

	initializeSelectedCashier(cashier: Cashier | null): void {
		this.#selectedCashier = cashier;
		
		// Add to cashiers list if not already there
		if (cashier && !this.#cashiers.find(c => c.cashierId === cashier.cashierId)) {
			this.#cashiers = [...this.#cashiers, cashier];
		}
	}

	async createCashier(cashierData: CreateCashierRequest): Promise<void> {
		this.#loading = true;
		this.#error = null;
		
		try {
			// Client-side stores should use form actions instead of direct API calls
			// This will be handled by the +page.server.ts actions
			const formData = new FormData();
			formData.append('name', cashierData.name);
			formData.append('email', cashierData.email);
			cashierData.currencies?.forEach(currency => {
				formData.append('currencies', currency);
			});
			
			const response = await fetch('?/default', {
				method: 'POST',
				body: formData
			});
			
			if (!response.ok) {
				throw new Error('Failed to create cashier');
			}
		} catch (error) {
			this.#error = (error as Error).message || 'Failed to create cashier';
			console.error('Failed to create cashier:', error);
			throw error;
		} finally {
			this.#loading = false;
		}
	}

	async updateCashier(id: string, cashierData: UpdateCashierRequest): Promise<void> {
		this.#loading = true;
		this.#error = null;
		
		try {
			// Client-side stores should use form actions instead of direct API calls
			// This will be handled by server actions in the detail page
			const formData = new FormData();
			formData.append('name', cashierData.name);
			formData.append('email', cashierData.email);
			cashierData.currencies?.forEach(currency => {
				formData.append('currencies', currency);
			});
			
			const response = await fetch(`/cashiers/${id}?/update`, {
				method: 'POST',
				body: formData
			});
			
			if (!response.ok) {
				throw new Error('Failed to update cashier');
			}
		} catch (error) {
			this.#error = (error as Error).message || 'Failed to update cashier';
			console.error('Failed to update cashier:', error);
			throw error;
		} finally {
			this.#loading = false;
		}
	}

	async deleteCashier(id: string): Promise<void> {
		this.#loading = true;
		this.#error = null;
		
		try {
			// Client-side stores should use form actions instead of direct API calls
			// This will be handled by server actions in the detail page
			const response = await fetch(`/cashiers/${id}?/delete`, {
				method: 'POST'
			});
			
			if (!response.ok) {
				throw new Error('Failed to delete cashier');
			}
			
			// Remove from the list
			this.#cashiers = this.#cashiers.filter(c => c.cashierId !== id);
			
			// Clear selected cashier if it's the same one
			if (this.#selectedCashier?.cashierId === id) {
				this.#selectedCashier = null;
			}
		} catch (error) {
			this.#error = (error as Error).message || 'Failed to delete cashier';
			console.error('Failed to delete cashier:', error);
			throw error;
		} finally {
			this.#loading = false;
		}
	}

	// Filter actions
	setSearchTerm(term: string): void {
		this.#searchTerm = term;
	}

	setCurrencyFilter(currency: string): void {
		this.#currencyFilter = currency;
	}

	clearFilters(): void {
		this.#searchTerm = '';
		this.#currencyFilter = 'all';
	}

	// Utility actions
	clearError(): void {
		this.#error = null;
	}

	clearSelectedCashier(): void {
		this.#selectedCashier = null;
	}

	// Get cashier by ID from current list
	getCashierById(id: string): Cashier | null {
		return this.#cashiers.find(c => c.cashierId === id) || null;
	}
}

// Create and export the singleton store instance
export const cashierStore = new CashierStore();