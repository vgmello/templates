<script lang="ts">
	import { formatCurrency, getCurrencySymbol } from "$lib/utils/currency.js";
	import { cn } from "$lib/utils.js";

	type Props = {
		amount: number;
		currency?: string;
		size?: 'sm' | 'md' | 'lg' | 'xl';
		variant?: 'default' | 'muted' | 'accent' | 'success' | 'warning' | 'destructive';
		showCode?: boolean;
		class?: string;
	};

	let {
		amount,
		currency = "USD",
		size = "md",
		variant = "default",
		showCode = false,
		class: className,
		...restProps
	}: Props = $props();

	// Size classes
	const sizeClasses = {
		sm: "text-sm",
		md: "text-base",
		lg: "text-lg font-semibold",
		xl: "text-2xl font-bold"
	};

	// Variant classes
	const variantClasses = {
		default: "text-foreground",
		muted: "text-muted-foreground",
		accent: "text-primary",
		success: "text-green-600",
		warning: "text-orange-600",
		destructive: "text-red-600"
	};

	let formattedAmount = $derived(formatCurrency(amount, currency));
	let currencySymbol = $derived(getCurrencySymbol(currency));
</script>

<span 
	class={cn(
		"font-mono tabular-nums",
		sizeClasses[size],
		variantClasses[variant],
		className
	)}
	title={`${formattedAmount} (${currency})`}
	{...restProps}
>
	{formattedAmount}
	{#if showCode && size !== 'sm'}
		<span class="text-xs text-muted-foreground ml-1 font-sans">
			{currency}
		</span>
	{/if}
</span>