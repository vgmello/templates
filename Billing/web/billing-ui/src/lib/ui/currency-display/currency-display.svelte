<script lang="ts">
	import { formatCurrency } from '$lib/infrastructure/utils/Currency.js';
	import { cn } from '$lib/infrastructure/utils/Utils.js';

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
		currency = 'USD',
		size = 'md',
		variant = 'default',
		showCode = false,
		class: className,
		...restProps
	}: Props = $props();

	// Size classes
	const sizeClasses = {
		sm: 'text-sm',
		md: 'text-base',
		lg: 'text-lg font-semibold',
		xl: 'text-2xl font-bold'
	};

	// Variant classes
	const variantClasses = {
		default: 'text-foreground',
		muted: 'text-muted-foreground',
		accent: 'text-primary',
		success: 'text-green-600',
		warning: 'text-orange-600',
		destructive: 'text-red-600'
	};

	let formattedAmount = $derived(formatCurrency(amount, currency));
</script>

<span
	class={cn('font-mono tabular-nums', sizeClasses[size], variantClasses[variant], className)}
	title={`${formattedAmount} (${currency})`}
	{...restProps}
>
	{formattedAmount}
	{#if showCode && size !== 'sm'}
		<span class="ml-1 font-sans text-xs text-muted-foreground">
			{currency}
		</span>
	{/if}
</span>
