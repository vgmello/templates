<script lang="ts">
	import { goto, invalidate } from '$app/navigation';
	import { enhance } from '$app/forms';
	import { Button } from '$lib/ui/button';
	import { Card, CardContent } from '$lib/ui/card';
	import { Input } from '$lib/ui/input';
	import {
		Plus,
		Search,
		Filter,
		Settings,
		UserPlus
	} from '@lucide/svelte';
	import { CashierCard, type GetCashiersResult } from '$lib/cashiers';
	import type { ActionData } from './$types';

	type Props = {
		data: {
			cashiers: GetCashiersResult[];
		};
		form: ActionData;
	};

	let { data, form }: Props = $props();
	let error = $state<string | null>(form?.errors?.[0] || null);
	let searchTerm = $state('');
	let deletingId = $state<string | null>(null);

	// Reactive filtered cashiers using simple search
	let filteredCashiers = $derived(
		data.cashiers.filter((cashier) => {
			// Simple text search on available fields
			const displayName = cashier.name || cashier.email || 'Unknown';
			const matchesSearch = searchTerm === '' || 
				displayName.toLowerCase().includes(searchTerm.toLowerCase()) ||
				cashier.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
				cashier.cashierId.toLowerCase().includes(searchTerm.toLowerCase());

			return matchesSearch;
		})
	);

	// Simple statistics
	let totalCashiers = $derived(data.cashiers.length);

	function editCashier(id: string) {
		goto(`/cashiers/${id}/edit`);
	}

	function createCashier() {
		goto('/cashiers/create');
	}

	function refreshCashiers() {
		invalidate('data');
	}
</script>

<svelte:head>
	<title>Cashier Management - Billing System</title>
</svelte:head>

<div class="container mx-auto space-y-8 p-6">
	<!-- Header Section with Clear Hierarchy -->
	<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
		<div class="space-y-1">
			<h1 class="text-3xl font-bold tracking-tight text-foreground">Cashiers</h1>
			<p class="text-muted-foreground">
				Manage cashiers and their payment configurations. 
				{filteredCashiers.length} cashier{filteredCashiers.length === 1 ? '' : 's'} found.
			</p>
		</div>
		<Button onclick={createCashier} class="flex h-10 items-center gap-2 px-6">
			<Plus size={18} />
			Add Cashier
		</Button>
	</div>

	<!-- Search and Filter Bar -->
	<div class="flex flex-col items-stretch gap-4 sm:flex-row sm:items-center">
		<div class="relative max-w-md flex-1">
			<Search
				size={16}
				class="absolute left-3 top-1/2 -translate-y-1/2 transform text-muted-foreground"
			/>
			<Input
				bind:value={searchTerm}
				placeholder="Search cashiers by name, email, or ID..."
				class="h-10 pl-10"
			/>
		</div>
		
		<div class="flex items-center gap-2">
			<span class="text-sm text-muted-foreground">
				{totalCashiers} cashier{totalCashiers === 1 ? '' : 's'}
			</span>
		</div>
	</div>

	<!-- Main Content Area -->
	{#if error}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="flex flex-col items-center justify-center space-y-4 py-12">
				<div class="rounded-full bg-destructive/10 p-3">
					<Settings size={24} class="text-destructive" />
				</div>
				<div class="space-y-2 text-center">
					<h3 class="font-semibold text-destructive">Something went wrong</h3>
					<p class="max-w-md text-sm text-muted-foreground">{error}</p>
				</div>
				<Button onclick={refreshCashiers} variant="outline" class="gap-2">
					<Settings size={16} />
					Try Again
				</Button>
			</CardContent>
		</Card>
	{:else if filteredCashiers.length === 0}
		<Card class="border-2 border-dashed">
			<CardContent class="flex flex-col items-center justify-center space-y-6 py-24">
				<div class="rounded-full bg-muted p-4">
					<UserPlus size={32} class="text-muted-foreground" />
				</div>
				<div class="max-w-md space-y-2 text-center">
					<h3 class="text-lg font-semibold">
						{searchTerm ? 'No matching cashiers' : 'No cashiers yet'}
					</h3>
					<p class="text-muted-foreground">
						{searchTerm
							? `No cashiers found matching "${searchTerm}". Try adjusting your search terms.`
							: 'Get started by adding your first cashier to manage payments and configurations.'}
					</p>
				</div>
				{#if !searchTerm}
					<Button onclick={createCashier} class="gap-2">
						<Plus size={16} />
						Add Your First Cashier
					</Button>
				{/if}
			</CardContent>
		</Card>
	{:else}
		<!-- Cashiers Grid -->
		<div class="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
			{#each filteredCashiers as cashier (cashier.id)}
				<form
					method="POST"
					action="?/delete"
					use:enhance={() => {
						if (
							!confirm(
								`Are you sure you want to delete cashier "${cashier.displayName}"?`
							)
						) {
							return ({ cancel }) => cancel();
						}
						deletingId = cashier.id;

						return async ({ result, update }) => {
							deletingId = null;
							if (result.type === 'success') {
								// Invalidate the current page's data to force a reload
								await invalidate('/cashiers');
								await invalidate('cashiers:list');
								// Also force update the page
								await update({ reset: false });
							} else {
								await update();
							}
						};
					}}
				>
					<input type="hidden" name="id" value={cashier.id} />
					<CashierCard
						{cashier}
						onEdit={editCashier}
						onDelete={(id) => {
							// The delete is handled by the form enhance above
							const form = document.querySelector(`form input[value="${id}"]`)?.closest('form');
							if (form) {
								form.requestSubmit();
							}
						}}
						deleting={deletingId === cashier.id}
					/>
				</form>
			{/each}
		</div>

		<!-- Summary Footer -->
		{#if filteredCashiers.length > 0}
			<div class="flex items-center justify-between border-t pt-6">
				<p class="text-sm text-muted-foreground">
					Showing {filteredCashiers.length} of {data.cashiers.length} cashiers
				</p>
			</div>
		{/if}
	{/if}
</div>
