<script lang="ts">
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import Button from "$lib/components/ui/button.svelte";
	import Card from "$lib/components/ui/card.svelte";
	import Badge from "$lib/components/ui/badge.svelte";
	import Input from "$lib/components/ui/input.svelte";
	import { Plus, User, Mail, CreditCard, Search, Filter, AlertCircle } from "lucide-svelte";
	import { cashierStore } from '$lib/stores/cashier.svelte';

	// Get SSR data
	let { data } = $props();

	// Initialize store with SSR data
	onMount(() => {
		cashierStore.initializeCashiers(data.cashiers);
	});

	// Direct access to store properties - they're already reactive
	// No need to wrap in $derived since the store properties are $state/$derived

	function handleCreateCashier() {
		goto('/cashiers/create');
	}

	function handleCashierClick(cashierId: string) {
		goto(`/cashiers/${cashierId}`);
	}

	function handleSearchChange(event: Event) {
		const target = event.target as HTMLInputElement;
		cashierStore.setSearchTerm(target.value);
	}

	function handleCurrencyFilterChange(event: Event) {
		const target = event.target as HTMLSelectElement;
		cashierStore.setCurrencyFilter(target.value);
	}

	function clearFilters() {
		cashierStore.clearFilters();
	}

	function dismissError() {
		cashierStore.clearError();
	}
</script>

<svelte:head>
	<title>Cashiers - Billing Service</title>
</svelte:head>

<div class="container mx-auto px-4 py-8">
	<div class="space-y-6">
		<!-- Error Banner -->
		{#if cashierStore.error}
			<Card class="p-4 border-destructive bg-destructive/10">
				<div class="flex items-center gap-3">
					<AlertCircle class="h-5 w-5 text-destructive" />
					<div class="flex-1">
						<p class="text-sm font-medium text-destructive">Error loading cashiers</p>
						<p class="text-sm text-destructive/80">{cashierStore.error}</p>
					</div>
					<Button onclick={dismissError} variant="ghost" size="sm">
						Dismiss
					</Button>
				</div>
			</Card>
		{/if}

		<!-- Header -->
		<div class="flex items-center justify-between">
			<div class="space-y-1">
				<h1 class="text-3xl font-bold tracking-tight">Cashiers</h1>
				<p class="text-muted-foreground">
					Manage cashiers and their payment configurations. {cashierStore.totalCashiers} total, {cashierStore.configuredCashiers} configured.
					{#if data.serviceUnavailable}
						<span class="text-yellow-600">(Service temporarily unavailable)</span>
					{/if}
				</p>
			</div>
			<Button onclick={handleCreateCashier} class="gap-2">
				<Plus class="h-4 w-4" />
				Add Cashier
			</Button>
		</div>

		<!-- Search and Filter -->
		{#if cashierStore.totalCashiers > 0}
			<Card class="p-4">
				<div class="flex flex-col sm:flex-row gap-4">
					<div class="flex-1 relative">
						<Search class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
						<Input
							type="text"
							placeholder="Search cashiers by name or email..."
							value={cashierStore.searchTerm}
							oninput={handleSearchChange}
							class="pl-10"
						/>
					</div>
					<div class="flex items-center gap-2">
						<Filter class="h-4 w-4 text-muted-foreground" />
						<select 
							value={cashierStore.currencyFilter}
							onchange={handleCurrencyFilterChange}
							class="h-10 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
						>
							<option value="all">All Currencies</option>
							{#each cashierStore.availableCurrencies as currency}
								<option value={currency}>{currency}</option>
							{/each}
						</select>
					</div>
				</div>
			</Card>
		{/if}

		<!-- Cashiers Grid -->
		{#if cashierStore.filteredCashiers.length === 0 && cashierStore.totalCashiers === 0}
				<Card class="p-12">
					<div class="text-center space-y-4">
						<User class="h-12 w-12 mx-auto text-muted-foreground" />
						<div class="space-y-2">
							<h3 class="text-lg font-semibold">No cashiers found</h3>
							<p class="text-sm text-muted-foreground max-w-sm mx-auto">
								Get started by creating your first cashier to handle payments.
							</p>
						</div>
						<Button onclick={handleCreateCashier} class="gap-2">
							<Plus class="h-4 w-4" />
							Create First Cashier
						</Button>
					</div>
				</Card>
		{:else if cashierStore.filteredCashiers.length === 0}
			<Card class="p-12">
				<div class="text-center space-y-4">
					<Search class="h-12 w-12 mx-auto text-muted-foreground" />
					<div class="space-y-2">
						<h3 class="text-lg font-semibold">No cashiers found</h3>
						<p class="text-sm text-muted-foreground max-w-sm mx-auto">
							No cashiers match your current search and filter criteria.
						</p>
					</div>
					<Button onclick={clearFilters} variant="outline">
						Clear Filters
					</Button>
				</div>
			</Card>
		{:else}
			<div class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
				{#each cashierStore.filteredCashiers as cashier, index (`${cashier.cashierId}-${index}`)}
					<Card class="p-6 hover:shadow-md transition-shadow cursor-pointer" onclick={() => handleCashierClick(cashier.cashierId)}>
						<div class="space-y-4">
							<div class="flex items-start justify-between">
								<div class="space-y-1">
									<h3 class="font-semibold flex items-center gap-2">
										<User class="h-4 w-4" />
										{cashier.name}
									</h3>
									<p class="text-sm text-muted-foreground flex items-center gap-2">
										<Mail class="h-3 w-3" />
										{cashier.email}
									</p>
								</div>
							</div>

							{#if cashier.cashierPayments && cashier.cashierPayments.length > 0}
								<div class="space-y-2">
									<p class="text-xs font-medium text-muted-foreground flex items-center gap-1">
										<CreditCard class="h-3 w-3" />
										Supported Currencies
									</p>
									<div class="flex flex-wrap gap-1">
										{#each cashier.cashierPayments as payment}
											<Badge variant="secondary" class="text-xs">
												{payment.currency}
											</Badge>
										{/each}
									</div>
								</div>
							{:else}
								<div class="space-y-2">
									<p class="text-xs font-medium text-muted-foreground">No currencies configured</p>
									<Badge variant="outline" class="text-xs">Setup Required</Badge>
								</div>
							{/if}

							<div class="flex items-center text-xs text-muted-foreground">
								<span>Created: {new Date(cashier.createdDateUtc || Date.now()).toLocaleDateString()}</span>
							</div>
						</div>
					</Card>
				{/each}
			</div>
		{/if}
	</div>
</div>