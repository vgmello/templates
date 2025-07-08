<script lang="ts">
	import { cn } from '$lib/infrastructure/utils/Utils.js';
	import { formatCurrency } from '$lib/infrastructure/utils/Currency.js';

	type Props = {
		id?: string;
		value: number;
		currency?: string;
		placeholder?: string;
		disabled?: boolean;
		class?: string;
		min?: number;
		max?: number;
		step?: number;
		required?: boolean;
		error?: string;
	};

	let {
		id,
		value = $bindable(0),
		currency = 'USD',
		placeholder = '0.00',
		disabled = false,
		class: className,
		min = 0,
		max,
		step = 0.01,
		required = false,
		error,
		...restProps
	}: Props = $props();

	let inputValue = $state('');
	let isFocused = $state(false);
	let lastExternalValue = $state(value);

	// Keep inputValue in sync with external value changes
	$effect(() => {
		if (value !== lastExternalValue && !isFocused) {
			lastExternalValue = value;
			inputValue = value > 0 ? value.toString() : '';
		}
	});

	// Initialize inputValue on first load
	$effect(() => {
		if (inputValue === '' && value > 0) {
			inputValue = value.toString();
		}
	});

	// Format the display value when not focused
	let displayValue = $derived(() => {
		if (isFocused) {
			return inputValue;
		}

		// Show formatted currency when not focused and has value
		if (value > 0) {
			try {
				return formatCurrency(value, currency).replace(/^\$/, ''); // Remove currency symbol since we show it separately
			} catch {
				// Fallback to basic formatting if formatCurrency fails
				return value.toFixed(2);
			}
		}

		return '';
	});

	function isValidNumber(num: number): boolean {
		return !isNaN(num) && 
			   isFinite(num) && 
			   num >= (min ?? 0) && 
			   (max === undefined || num <= max);
	}

	function handleFocus() {
		isFocused = true;
		// Show the actual numeric value for editing
		inputValue = value > 0 ? value.toString() : '';
	}

	function handleBlur() {
		isFocused = false;
		
		// Parse and validate the input value
		const trimmed = inputValue.trim();
		if (trimmed === '') {
			value = 0;
			inputValue = '';
			lastExternalValue = 0;
			return;
		}

		const numericValue = parseFloat(trimmed);
		if (isValidNumber(numericValue)) {
			value = numericValue;
			lastExternalValue = numericValue;
		} else {
			// Revert to last valid value if input is invalid
			inputValue = value > 0 ? value.toString() : '';
		}
	}

	function handleInput(event: Event) {
		const target = event.target as HTMLInputElement;
		inputValue = target.value;

		// Only update value in real-time if it's a valid number
		// This prevents flicker and maintains better UX
		const numericValue = parseFloat(target.value);
		if (isValidNumber(numericValue)) {
			value = numericValue;
			lastExternalValue = numericValue;
		}
	}

	// Currency symbol mapping
	const currencySymbols: Record<string, string> = {
		USD: '$',
		EUR: '€',
		GBP: '£',
		JPY: '¥',
		CAD: 'C$',
		AUD: 'A$'
	};

	let currencySymbol = $derived(currencySymbols[currency] || currency);
</script>

<div class="relative">
	<div class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
		{currencySymbol}
	</div>

	<input
		{id}
		type="number"
		{min}
		{max}
		{step}
		{placeholder}
		{disabled}
		{required}
		value={displayValue}
		onfocus={handleFocus}
		onblur={handleBlur}
		oninput={handleInput}
		aria-label={restProps['aria-label'] || `Amount in ${currency}`}
		aria-describedby={error ? `${id}-error` : undefined}
		aria-invalid={error ? 'true' : 'false'}
		role="spinbutton"
		aria-valuemin={min}
		aria-valuemax={max}
		aria-valuenow={value}
		aria-valuetext={value > 0 ? `${value} ${currency}` : undefined}
		class={cn(
			'flex h-10 w-full rounded-md border border-input bg-background py-2 pl-8 pr-3 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
			error && 'border-destructive focus-visible:ring-destructive',
			className
		)}
		{...restProps}
	/>

	{#if !isFocused && value > 0}
		<div
			class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-xs text-muted-foreground"
		>
			{currency}
		</div>
	{/if}
</div>

{#if error}
	<p id="{id}-error" class="mt-1 text-sm text-destructive" role="alert" aria-live="polite">
		{error}
	</p>
{/if}
