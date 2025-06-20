import { cashierService } from '$lib/api.js';

/**
 * @typedef {Object} Cashier
 * @property {string} cashierId
 * @property {string} name
 * @property {string} email
 * @property {string} [createdDateUtc]
 * @property {string} [updatedDateUtc]
 * @property {number} [version]
 * @property {Array<{currency: string, isActive?: boolean, createdDateUtc?: string}>} [cashierPayments]
 */

/**
 * @typedef {Object} CashierStore
 * @property {Cashier[]} cashiers - Array of all cashiers
 * @property {Cashier | null} selectedCashier - Currently selected cashier
 * @property {boolean} loading - Loading state
 * @property {string | null} error - Error message
 * @property {string} searchTerm - Search filter
 * @property {string} currencyFilter - Currency filter
 */

class CashierStore {
	// Private state
	#cashiers = $state([]);
	#selectedCashier = $state(null);
	#loading = $state(false);
	#error = $state(null);
	#searchTerm = $state('');
	#currencyFilter = $state('all');

	// Public getters
	get cashiers() {
		return this.#cashiers;
	}

	get selectedCashier() {
		return this.#selectedCashier;
	}

	get loading() {
		return this.#loading;
	}

	get error() {
		return this.#error;
	}

	get searchTerm() {
		return this.#searchTerm;
	}

	get currencyFilter() {
		return this.#currencyFilter;
	}

	// Computed properties
	get filteredCashiers() {
		return $derived.by(() => {
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
		});
	}

	get totalCashiers() {
		return $derived(this.#cashiers.length);
	}

	get configuredCashiers() {
		return $derived(
			this.#cashiers.filter(cashier => 
				cashier.cashierPayments && cashier.cashierPayments.length > 0
			).length
		);
	}

	get availableCurrencies() {
		return $derived.by(() => {
			const currencies = new Set();
			this.#cashiers.forEach(cashier => {
				cashier.cashierPayments?.forEach(payment => {
					currencies.add(payment.currency);
				});
			});
			return Array.from(currencies).sort();
		});
	}

	// Actions
	async loadCashiers(fetchFn = fetch) {
		this.#loading = true;
		this.#error = null;
		
		try {
			const cashiers = await cashierService.getCashiers(fetchFn);
			this.#cashiers = cashiers;
		} catch (error) {
			this.#error = error.message || 'Failed to load cashiers';
			console.error('Failed to load cashiers:', error);
		} finally {
			this.#loading = false;
		}
	}

	async loadCashier(id, fetchFn = fetch) {
		this.#loading = true;
		this.#error = null;
		
		try {
			const cashier = await cashierService.getCashier(id, fetchFn);
			this.#selectedCashier = cashier;
			
			// Update the cashier in the list if it exists
			const index = this.#cashiers.findIndex(c => c.cashierId === id);
			if (index !== -1) {
				this.#cashiers[index] = cashier;
			}
			
			return cashier;
		} catch (error) {
			this.#error = error.message || 'Failed to load cashier';
			console.error('Failed to load cashier:', error);
			throw error;
		} finally {
			this.#loading = false;
		}
	}

	async createCashier(cashierData, fetchFn = fetch) {
		this.#loading = true;
		this.#error = null;
		
		try {
			const newCashier = await cashierService.createCashier(cashierData, fetchFn);
			this.#cashiers = [...this.#cashiers, newCashier];
			return newCashier;
		} catch (error) {
			this.#error = error.message || 'Failed to create cashier';
			console.error('Failed to create cashier:', error);
			throw error;
		} finally {
			this.#loading = false;
		}
	}

	async updateCashier(id, cashierData, fetchFn = fetch) {
		this.#loading = true;
		this.#error = null;
		
		try {
			const updatedCashier = await cashierService.updateCashier(id, cashierData, fetchFn);
			
			// Update in the list
			const index = this.#cashiers.findIndex(c => c.cashierId === id);
			if (index !== -1) {
				this.#cashiers[index] = updatedCashier;
			}
			
			// Update selected cashier if it's the same one
			if (this.#selectedCashier?.cashierId === id) {
				this.#selectedCashier = updatedCashier;
			}
			
			return updatedCashier;
		} catch (error) {
			this.#error = error.message || 'Failed to update cashier';
			console.error('Failed to update cashier:', error);
			throw error;
		} finally {
			this.#loading = false;
		}
	}

	async deleteCashier(id, fetchFn = fetch) {
		this.#loading = true;
		this.#error = null;
		
		try {
			await cashierService.deleteCashier(id, fetchFn);
			
			// Remove from the list
			this.#cashiers = this.#cashiers.filter(c => c.cashierId !== id);
			
			// Clear selected cashier if it's the same one
			if (this.#selectedCashier?.cashierId === id) {
				this.#selectedCashier = null;
			}
		} catch (error) {
			this.#error = error.message || 'Failed to delete cashier';
			console.error('Failed to delete cashier:', error);
			throw error;
		} finally {
			this.#loading = false;
		}
	}

	// Filter actions
	setSearchTerm(term) {
		this.#searchTerm = term;
	}

	setCurrencyFilter(currency) {
		this.#currencyFilter = currency;
	}

	clearFilters() {
		this.#searchTerm = '';
		this.#currencyFilter = 'all';
	}

	// Utility actions
	clearError() {
		this.#error = null;
	}

	clearSelectedCashier() {
		this.#selectedCashier = null;
	}

	// Get cashier by ID from current list
	getCashierById(id) {
		return this.#cashiers.find(c => c.cashierId === id) || null;
	}
}

// Create and export the singleton store instance
export const cashierStore = new CashierStore();