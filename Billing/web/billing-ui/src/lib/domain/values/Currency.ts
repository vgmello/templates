export const SupportedCurrencies = {
	USD: 'USD',
	EUR: 'EUR',
	GBP: 'GBP',
	JPY: 'JPY',
	CAD: 'CAD',
	AUD: 'AUD',
	CHF: 'CHF',
	CNY: 'CNY'
} as const;

export type Currency = (typeof SupportedCurrencies)[keyof typeof SupportedCurrencies];

export class CurrencyValue {
	constructor(public readonly code: Currency) {
		if (!Object.values(SupportedCurrencies).includes(code)) {
			throw new Error(`Unsupported currency: ${code}`);
		}
	}

	getSymbol(): string {
		const symbols: Record<Currency, string> = {
			USD: '$',
			EUR: '€',
			GBP: '£',
			JPY: '¥',
			CAD: 'C$',
			AUD: 'A$',
			CHF: 'CHF',
			CNY: '¥'
		};
		return symbols[this.code];
	}

	getName(): string {
		const names: Record<Currency, string> = {
			USD: 'US Dollar',
			EUR: 'Euro',
			GBP: 'British Pound',
			JPY: 'Japanese Yen',
			CAD: 'Canadian Dollar',
			AUD: 'Australian Dollar',
			CHF: 'Swiss Franc',
			CNY: 'Chinese Yuan'
		};
		return names[this.code];
	}

	getDecimalPlaces(): number {
		// JPY doesn't use decimal places
		return this.code === 'JPY' ? 0 : 2;
	}

	format(amount: number): string {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: this.code,
			minimumFractionDigits: this.getDecimalPlaces(),
			maximumFractionDigits: this.getDecimalPlaces()
		}).format(amount / 100);
	}

	static isValid(code: string): boolean {
		return Object.values(SupportedCurrencies).includes(code as Currency);
	}

	static all(): Currency[] {
		return Object.values(SupportedCurrencies);
	}
}