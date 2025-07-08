<script lang="ts">
	import { Badge } from '$lib/ui/badge';
	import type { Cashier } from '../models/Cashier';

	type Props = {
		cashier: Cashier;
		size?: 'sm' | 'default';
	};

	let { cashier, size = 'default' }: Props = $props();

	let statusConfig = $derived.by(() => {
		if (!cashier.isActive) {
			return { 
				text: 'Inactive', 
				variant: 'secondary' as const,
				description: 'This cashier is currently inactive'
			};
		}
		
		if (cashier.supportedCurrencies.length === 0) {
			return { 
				text: 'Setup Required', 
				variant: 'destructive' as const,
				description: 'This cashier needs to have supported currencies configured'
			};
		}
		
		return { 
			text: 'Active', 
			variant: 'default' as const,
			description: 'This cashier is active and ready to process transactions'
		};
	});
</script>

<Badge 
	variant={statusConfig.variant} 
	class={size === 'sm' ? 'text-xs' : ''}
	title={statusConfig.description}
>
	{statusConfig.text}
</Badge>