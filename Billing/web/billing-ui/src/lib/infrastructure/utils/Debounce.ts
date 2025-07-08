/**
 * Debounce utility for performance optimization
 */
export function debounce<T extends (...args: any[]) => any>(
	func: T,
	wait: number
): (...args: Parameters<T>) => void {
	let timeoutId: ReturnType<typeof setTimeout> | null = null;

	return (...args: Parameters<T>) => {
		// Clear existing timeout
		if (timeoutId !== null) {
			clearTimeout(timeoutId);
		}

		// Set new timeout
		timeoutId = setTimeout(() => {
			func(...args);
			timeoutId = null;
		}, wait);
	};
}

/**
 * Throttle utility for limiting function calls
 */
export function throttle<T extends (...args: any[]) => any>(
	func: T,
	limit: number
): (...args: Parameters<T>) => void {
	let inThrottle = false;

	return (...args: Parameters<T>) => {
		if (!inThrottle) {
			func(...args);
			inThrottle = true;
			setTimeout(() => {
				inThrottle = false;
			}, limit);
		}
	};
}

/**
 * Creates a debounced reactive search function for Svelte 5
 * Note: This function must be called from within a .svelte component
 */
export function createDebouncedSearch<T>(
	items: T[],
	searchFunction: (items: T[], term: string) => T[],
	delay: number = 300
) {
	// Import the rune dynamically to avoid issues in non-Svelte contexts
	// This will be called from within a .svelte component context
	const { state, effect, derived } = (() => {
		try {
			// These will only work inside .svelte components
			return {
				state: (initialValue: any) => {
					// @ts-ignore - This is called from within Svelte context
					return $state(initialValue);
				},
				effect: (fn: () => void | (() => void)) => {
					// @ts-ignore - This is called from within Svelte context  
					return $effect(fn);
				},
				derived: (fn: () => any) => {
					// @ts-ignore - This is called from within Svelte context
					return $derived(fn());
				}
			};
		} catch {
			// Fallback for non-Svelte contexts (shouldn't happen in practice)
			return {
				state: (initialValue: any) => initialValue,
				effect: (fn: () => void | (() => void)) => {},
				derived: (fn: () => any) => fn()
			};
		}
	})();

	let searchTerm = state('');
	let debouncedTerm = state('');
	let timeoutId: ReturnType<typeof setTimeout> | null = null;

	// Effect to debounce search term updates
	effect(() => {
		if (timeoutId) {
			clearTimeout(timeoutId);
		}

		timeoutId = setTimeout(() => {
			debouncedTerm = searchTerm;
		}, delay);

		// Cleanup function
		return () => {
			if (timeoutId) {
				clearTimeout(timeoutId);
			}
		};
	});

	// Filtered results based on debounced term
	let filteredItems = derived(() => searchFunction(items, debouncedTerm));

	return {
		get searchTerm() {
			return searchTerm;
		},
		set searchTerm(value: string) {
			searchTerm = value;
		},
		get filteredItems() {
			return filteredItems;
		},
		get isSearching() {
			return searchTerm !== debouncedTerm;
		}
	};
}