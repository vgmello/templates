<script lang="ts">
	import { goto } from '$app/navigation';
	import { CashierForm, Cashier } from '$lib/cashiers';
	import type { ActionData } from './$types';

	type Props = {
		form: ActionData;
	};

	let { form }: Props = $props();

	let cashier = $state(new Cashier());
	let loading = $state(false);

	function handleCancel() {
		goto('/cashiers');
	}
</script>

<svelte:head>
	<title>Create Cashier - Billing System</title>
</svelte:head>

<CashierForm
	bind:cashier
	{form}
	{loading}
	onCancel={handleCancel}
	submitLabel="Create Cashier"
	title="Create Cashier"
	description="Add a new cashier to the system"
/>