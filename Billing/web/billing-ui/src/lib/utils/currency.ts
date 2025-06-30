/**
 * Format currency amount with proper localization
 */
export function formatCurrency(amount: number, currency: string = 'USD'): string {
	try {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: currency.toUpperCase(),
			minimumFractionDigits: 2,
			maximumFractionDigits: 2,
		}).format(amount);
	} catch {
		// Fallback if currency is invalid
		return `${currency} ${amount.toFixed(2)}`;
	}
}

/**
 * Parse currency string to number (removes symbols and formatting)
 */
export function parseCurrency(currencyString: string): number {
	const numericString = currencyString.replace(/[^0-9.-]/g, '');
	return parseFloat(numericString) || 0;
}

/**
 * Get currency symbol for a given currency code
 */
export function getCurrencySymbol(currency: string = 'USD'): string {
	try {
		const formatter = new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: currency.toUpperCase(),
			minimumFractionDigits: 0,
			maximumFractionDigits: 0,
		});
		
		// Format 0 and extract just the symbol
		const formatted = formatter.format(0);
		return formatted.replace(/\d/g, '').trim();
	} catch {
		return currency.toUpperCase();
	}
}