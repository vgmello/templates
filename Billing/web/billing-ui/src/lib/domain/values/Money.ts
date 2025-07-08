export class Money {
	constructor(
		public readonly amount: number,
		public readonly currency: string
	) {
		if (amount < 0) {
			throw new Error('Money amount cannot be negative');
		}
		if (!currency || currency.length !== 3) {
			throw new Error('Currency must be a 3-letter code');
		}
	}

	add(other: Money): Money {
		if (this.currency !== other.currency) {
			throw new Error('Cannot add money with different currencies');
		}
		return new Money(this.amount + other.amount, this.currency);
	}

	subtract(other: Money): Money {
		if (this.currency !== other.currency) {
			throw new Error('Cannot subtract money with different currencies');
		}
		if (this.amount < other.amount) {
			throw new Error('Insufficient funds');
		}
		return new Money(this.amount - other.amount, this.currency);
	}

	multiply(factor: number): Money {
		if (factor < 0) {
			throw new Error('Cannot multiply money by negative factor');
		}
		return new Money(this.amount * factor, this.currency);
	}

	equals(other: Money): boolean {
		return this.amount === other.amount && this.currency === other.currency;
	}

	isZero(): boolean {
		return this.amount === 0;
	}

	format(): string {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: this.currency,
			minimumFractionDigits: 2,
			maximumFractionDigits: 2
		}).format(this.amount / 100);
	}

	static fromCents(cents: number, currency: string): Money {
		return new Money(cents, currency);
	}

	static zero(currency: string): Money {
		return new Money(0, currency);
	}
}