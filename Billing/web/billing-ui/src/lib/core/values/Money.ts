/**
 * Money class using integer-based arithmetic to avoid floating-point precision issues.
 * All amounts are stored as the smallest currency unit (e.g., cents for USD).
 */
export class Money {
	/**
	 * Creates a Money instance.
	 * @param cents - Amount in the smallest currency unit (e.g., cents for USD)
	 * @param currency - 3-letter ISO currency code
	 */
	constructor(
		public readonly cents: number,
		public readonly currency: string
	) {
		// Validate cents is an integer
		if (!Number.isInteger(cents)) {
			throw new Error('Money amount must be an integer representing the smallest currency unit');
		}
		if (cents < 0) {
			throw new Error('Money amount cannot be negative');
		}
		if (!currency || currency.length !== 3) {
			throw new Error('Currency must be a 3-letter ISO code');
		}
		// Validate currency format
		if (!/^[A-Z]{3}$/.test(currency)) {
			throw new Error('Currency must be uppercase 3-letter ISO code');
		}
	}

	/**
	 * @deprecated Use cents property instead. This maintains backward compatibility.
	 */
	get amount(): number {
		return this.cents;
	}

	add(other: Money): Money {
		if (this.currency !== other.currency) {
			throw new Error(`Cannot add money with different currencies: ${this.currency} and ${other.currency}`);
		}
		// Integer addition - no floating point issues
		return new Money(this.cents + other.cents, this.currency);
	}

	subtract(other: Money): Money {
		if (this.currency !== other.currency) {
			throw new Error(`Cannot subtract money with different currencies: ${this.currency} and ${other.currency}`);
		}
		if (this.cents < other.cents) {
			throw new Error('Insufficient funds');
		}
		// Integer subtraction - no floating point issues
		return new Money(this.cents - other.cents, this.currency);
	}

	multiply(factor: number): Money {
		if (factor < 0) {
			throw new Error('Cannot multiply money by negative factor');
		}
		// Use Math.round to handle floating point factors safely
		const result = Math.round(this.cents * factor);
		return new Money(result, this.currency);
	}

	divide(divisor: number): Money {
		if (divisor <= 0) {
			throw new Error('Cannot divide money by zero or negative number');
		}
		// Use Math.round for integer division result
		const result = Math.round(this.cents / divisor);
		return new Money(result, this.currency);
	}

	equals(other: Money): boolean {
		return this.cents === other.cents && this.currency === other.currency;
	}

	isZero(): boolean {
		return this.cents === 0;
	}

	isPositive(): boolean {
		return this.cents > 0;
	}

	isGreaterThan(other: Money): boolean {
		if (this.currency !== other.currency) {
			throw new Error(`Cannot compare money with different currencies: ${this.currency} and ${other.currency}`);
		}
		return this.cents > other.cents;
	}

	isLessThan(other: Money): boolean {
		if (this.currency !== other.currency) {
			throw new Error(`Cannot compare money with different currencies: ${this.currency} and ${other.currency}`);
		}
		return this.cents < other.cents;
	}

	/**
	 * Formats the money amount for display
	 */
	format(): string {
		// Convert cents to major currency unit for formatting
		const majorUnit = this.cents / 100;
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: this.currency,
			minimumFractionDigits: 2,
			maximumFractionDigits: 2
		}).format(majorUnit);
	}

	/**
	 * Returns the amount as a decimal string (e.g., "12.34" for 1234 cents)
	 */
	toDecimalString(): string {
		const majorUnit = this.cents / 100;
		return majorUnit.toFixed(2);
	}

	/**
	 * Creates Money from cents (smallest currency unit)
	 */
	static fromCents(cents: number, currency: string): Money {
		return new Money(cents, currency);
	}

	/**
	 * Creates Money from a decimal amount (e.g., 12.34 becomes 1234 cents)
	 */
	static fromDecimal(amount: number, currency: string): Money {
		if (!Number.isFinite(amount)) {
			throw new Error('Amount must be a finite number');
		}
		// Convert to cents and round to avoid floating point issues
		const cents = Math.round(amount * 100);
		return new Money(cents, currency);
	}

	/**
	 * Creates Money from a string representation (e.g., "12.34")
	 */
	static fromString(amountStr: string, currency: string): Money {
		const amount = parseFloat(amountStr);
		if (!Number.isFinite(amount)) {
			throw new Error(`Invalid amount string: ${amountStr}`);
		}
		return Money.fromDecimal(amount, currency);
	}

	static zero(currency: string): Money {
		return new Money(0, currency);
	}
}
