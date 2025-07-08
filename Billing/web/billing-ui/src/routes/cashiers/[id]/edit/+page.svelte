<script lang="ts">
	import { goto } from '$app/navigation';
	import { CashierForm } from '$lib/cashiers';
	import type { PageData, ActionData } from './$types';

	type Props = {
		data: PageData;
		form: ActionData;
	};

	let { data, form }: Props = $props();
	let loading = $state(false);

	// Initialize cashier with data from server (already a domain model)
	let cashier = $state(data.cashier);

	function handleCancel() {
		goto('/cashiers');
	}
</script>

<svelte:head>
	<title>Edit {cashier.displayName} - Billing System</title>
</svelte:head>

<CashierForm
	bind:cashier
	{form}
	{loading}
	onCancel={handleCancel}
	submitLabel="Update Cashier"
	title="Edit Cashier"
	description="Update cashier information"
/>