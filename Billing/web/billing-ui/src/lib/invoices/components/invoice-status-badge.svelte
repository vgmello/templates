<script lang="ts">
	import { Badge } from '$lib/ui/badge';
	import { InvoiceStatuses } from '$lib/core/values/InvoiceStatus.js';

	let { status, class: className = '' } = $props<{
		status: string;
		class?: string;
	}>();

	// Determine badge variant and additional styling based on status
	let badgeVariant = $derived(
		(() => {
			switch (status.toLowerCase()) {
				case InvoiceStatuses.PAID:
					return 'default' as const;
				case InvoiceStatuses.PENDING:
					return 'secondary' as const;
				case InvoiceStatuses.DRAFT:
					return 'outline' as const;
				case InvoiceStatuses.CANCELLED:
					return 'destructive' as const;
				case InvoiceStatuses.OVERDUE:
					return 'default' as const;
				default:
					return 'outline' as const;
			}
		})()
	);

	let badgeClass = $derived(() => {
		let baseClass = '';

		switch (status.toLowerCase()) {
			case InvoiceStatuses.PAID:
				baseClass = 'bg-green-100 text-green-800 hover:bg-green-100';
				break;
			case InvoiceStatuses.PENDING:
				baseClass = 'bg-blue-100 text-blue-800 hover:bg-blue-100';
				break;
			case InvoiceStatuses.DRAFT:
				baseClass = 'bg-gray-100 text-gray-800 hover:bg-gray-100';
				break;
			case InvoiceStatuses.CANCELLED:
				baseClass = 'bg-red-100 text-red-800 hover:bg-red-100';
				break;
			case InvoiceStatuses.OVERDUE:
				baseClass = 'bg-orange-100 text-orange-800 hover:bg-orange-100';
				break;
			default:
				baseClass = 'bg-gray-100 text-gray-800 hover:bg-gray-100';
		}

		return `${baseClass} ${className}`;
	});

	// Get status display text (capitalize first letter)
	let displayStatus = $derived(status.charAt(0).toUpperCase() + status.slice(1).toLowerCase());
</script>

<Badge variant={badgeVariant} class={badgeClass}>
	{displayStatus}
</Badge>
