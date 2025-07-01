<script lang="ts">
	import { cn } from "$lib/utils.js";
	import { formatCurrency } from "$lib/utils/currency.js";

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
		currency = "USD",
		placeholder = "0.00",
		disabled = false,
		class: className,
		min = 0,
		max,
		step = 0.01,
		required = false,
		error,
		...restProps
	}: Props = $props();

	let inputValue = $state(value.toString());
	let isFocused = $state(false);

	// Format the display value when not focused
	let displayValue = $derived(() => {
		if (isFocused) {
			return inputValue;
		}
		
		// Show formatted currency when not focused and has value
		if (value > 0) {
			return formatCurrency(value, currency).replace(/^\$/, ''); // Remove currency symbol since we show it separately
		}
		
		return '';
	});

	function handleFocus() {
		isFocused = true;
		inputValue = value > 0 ? value.toString() : '';
	}

	function handleBlur() {
		isFocused = false;
		// Parse and validate the input value
		const numericValue = parseFloat(inputValue);
		if (!isNaN(numericValue) && numericValue >= (min ?? 0)) {
			value = numericValue;
		} else {
			value = 0;
			inputValue = '';
		}
	}

	function handleInput(event: Event) {
		const target = event.target as HTMLInputElement;
		inputValue = target.value;
		
		// Update value in real-time for reactive forms
		const numericValue = parseFloat(target.value);
		if (!isNaN(numericValue) && numericValue >= (min ?? 0)) {
			value = numericValue;
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
	<div class="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground pointer-events-none">
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
		class={cn(
			"flex h-10 w-full rounded-md border border-input bg-background pl-8 pr-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
			error && "border-destructive focus-visible:ring-destructive",
			className
		)}
		{...restProps}
	/>
	
	{#if !isFocused && value > 0}
		<div class="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-muted-foreground pointer-events-none">
			{currency}
		</div>
	{/if}
</div>

{#if error}
	<p class="text-sm text-destructive mt-1">{error}</p>
{/if}