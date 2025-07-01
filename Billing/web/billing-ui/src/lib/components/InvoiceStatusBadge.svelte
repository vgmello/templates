<script lang="ts">
	import { Badge } from '$lib/components/ui/badge';
	import { InvoiceStatus } from '$lib/types/invoice.js';

	let { status, class: className = '' } = $props<{
		status: string;
		class?: string;
	}>();

	// Determine badge variant and additional styling based on status
	let badgeVariant = $derived((() => {
		switch (status) {
			case InvoiceStatus.PAID:
				return 'default';
			case InvoiceStatus.PENDING:
				return 'secondary';
			case InvoiceStatus.DRAFT:
				return 'outline';
			case InvoiceStatus.CANCELLED:
				return 'destructive';
			case InvoiceStatus.OVERDUE:
				return 'default';
			default:
				return 'outline';
		}
	})());

	let badgeClass = $derived(() => {
		let baseClass = '';
		
		switch (status) {
			case InvoiceStatus.PAID:
				baseClass = 'bg-green-100 text-green-800 hover:bg-green-100';
				break;
			case InvoiceStatus.PENDING:
				baseClass = 'bg-blue-100 text-blue-800 hover:bg-blue-100';
				break;
			case InvoiceStatus.DRAFT:
				baseClass = 'bg-gray-100 text-gray-800 hover:bg-gray-100';
				break;
			case InvoiceStatus.CANCELLED:
				baseClass = 'bg-red-100 text-red-800 hover:bg-red-100';
				break;
			case InvoiceStatus.OVERDUE:
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