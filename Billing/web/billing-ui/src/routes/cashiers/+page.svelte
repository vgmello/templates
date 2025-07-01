<script lang="ts">
	import { goto } from '$app/navigation';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { Plus, Search, Pencil, Trash2, User, Mail, Hash, Settings, UserPlus } from '@lucide/svelte';
	import { cashierApi, type GetCashiersResult } from '$lib';

	type Props = {
		data: {
			cashiers: GetCashiersResult[];
		};
	};
	
	let { data }: Props = $props();
	let cashiers = $state<GetCashiersResult[]>(data.cashiers);
	let error = $state<string | null>(null);
	let searchTerm = $state('');

	// Reactive filtered cashiers
	let filteredCashiers = $derived(
		cashiers.filter(cashier => 
			cashier.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
			cashier.email.toLowerCase().includes(searchTerm.toLowerCase())
		)
	);

	async function refreshCashiers() {
		try {
			cashiers = await cashierApi.getCashiers();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to refresh cashiers';
			console.error('Error refreshing cashiers:', err);
		}
	}

	async function deleteCashier(id: string, name: string) {
		if (!confirm(`Are you sure you want to delete cashier "${name}"?`)) {
			return;
		}

		try {
			await cashierApi.deleteCashier(id);
			await refreshCashiers(); // Reload the list
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to delete cashier';
			console.error('Error deleting cashier:', err);
		}
	}

	function editCashier(id: string) {
		goto(`/cashiers/${id}/edit`);
	}

	function createCashier() {
		goto('/cashiers/create');
	}

</script>

<svelte:head>
	<title>Cashier Management - Billing System</title>
</svelte:head>

<div class="container mx-auto p-6 space-y-8">
	<!-- Header Section with Clear Hierarchy -->
	<div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
		<div class="space-y-1">
			<h1 class="text-3xl font-bold tracking-tight text-foreground">Cashiers</h1>
			<p class="text-muted-foreground">Manage cashiers and their payment configurations. {filteredCashiers.length} total, {filteredCashiers.filter(c => c.email).length} configured.</p>
		</div>
		<Button onclick={createCashier} class="flex items-center gap-2 h-10 px-6">
			<Plus size={18} />
			Add Cashier
		</Button>
	</div>

	<!-- Search and Filter Bar -->
	<div class="flex flex-col sm:flex-row gap-4 items-stretch sm:items-center">
		<div class="relative flex-1 max-w-md">
			<Search size={16} class="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground" />
			<Input
				bind:value={searchTerm}
				placeholder="Search cashiers by name or email..."
				class="pl-10 h-10"
			/>
		</div>
		<div class="flex items-center gap-2 text-sm text-muted-foreground">
			<span class="flex items-center gap-1 px-2 py-1 bg-secondary rounded-md text-secondary-foreground">
				<User size={12} />
				All Currencies
			</span>
		</div>
	</div>

	<!-- Main Content Area -->
	{#if error}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="flex flex-col items-center justify-center py-12 space-y-4">
				<div class="rounded-full bg-destructive/10 p-3">
					<Settings size={24} class="text-destructive" />
				</div>
				<div class="text-center space-y-2">
					<h3 class="font-semibold text-destructive">Something went wrong</h3>
					<p class="text-sm text-muted-foreground max-w-md">{error}</p>
				</div>
				<Button onclick={refreshCashiers} variant="outline" class="gap-2">
					<Settings size={16} />
					Try Again
				</Button>
			</CardContent>
		</Card>
	{:else if filteredCashiers.length === 0}
		<Card class="border-dashed border-2">
			<CardContent class="flex flex-col items-center justify-center py-24 space-y-6">
				<div class="rounded-full bg-muted p-4">
					<UserPlus size={32} class="text-muted-foreground" />
				</div>
				<div class="text-center space-y-2 max-w-md">
					<h3 class="text-lg font-semibold">
						{searchTerm ? 'No matching cashiers' : 'No cashiers yet'}
					</h3>
					<p class="text-muted-foreground">
						{searchTerm 
							? `No cashiers found matching "${searchTerm}". Try adjusting your search terms.`
							: 'Get started by adding your first cashier to manage payments and configurations.'
						}
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
		<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
			{#each filteredCashiers as cashier}
				<Card class="group hover:shadow-lg transition-all duration-200 hover:border-primary/20 bg-card">
					<CardContent class="p-6">
						<!-- Card Header with Avatar and Status -->
						<div class="flex items-start justify-between mb-4">
							<div class="flex items-start gap-3">
								<div class="rounded-full bg-primary/10 p-2 group-hover:bg-primary/20 transition-colors">
									<User size={20} class="text-primary" />
								</div>
								<div class="space-y-1 flex-1 min-w-0">
									<h3 class="font-semibold text-foreground truncate" title={cashier.name}>
										{cashier.name}
									</h3>
									<div class="flex items-center gap-1">
										{#if cashier.email}
											<span class="px-2 py-0.5 bg-orange-100 text-orange-800 text-xs font-medium rounded-full">
												Setup Required
											</span>
										{:else}
											<span class="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs font-medium rounded-full">
												No currencies configured
											</span>
										{/if}
									</div>
								</div>
							</div>
						</div>

						<!-- Cashier Details -->
						<div class="space-y-3 mb-6">
							<!-- Email -->
							<div class="flex items-center gap-2 text-sm">
								<Mail size={14} class="text-muted-foreground flex-shrink-0" />
								<span class="text-muted-foreground truncate">
									{cashier.email || 'No email provided'}
								</span>
							</div>

							<!-- Cashier ID -->
							<div class="flex items-center gap-2 text-sm">
								<Hash size={14} class="text-muted-foreground flex-shrink-0" />
								<span class="font-mono text-muted-foreground text-xs truncate" title={cashier.cashierId}>
									{cashier.cashierId}
								</span>
							</div>
						</div>

						<!-- Created Date -->
						<div class="text-xs text-muted-foreground mb-4 border-t pt-3">
							Created: {new Date().toLocaleDateString('en-US', { 
								month: 'short', 
								day: 'numeric', 
								year: 'numeric' 
							})}
						</div>

						<!-- Action Buttons -->
						<div class="flex gap-2">
							<Button
								size="sm"
								variant="outline"
								onclick={() => editCashier(cashier.cashierId)}
								class="flex-1 gap-2 group-hover:border-primary/30 transition-colors"
							>
								<Pencil size={14} />
								Edit
							</Button>
							<Button
								size="sm"
								variant="ghost"
								onclick={() => deleteCashier(cashier.cashierId, cashier.name)}
								class="text-destructive hover:text-destructive hover:bg-destructive/10"
							>
								<Trash2 size={14} />
							</Button>
						</div>
					</CardContent>
				</Card>
			{/each}
		</div>

		<!-- Summary Footer -->
		{#if filteredCashiers.length > 0}
			<div class="flex items-center justify-between pt-6 border-t">
				<p class="text-sm text-muted-foreground">
					Showing {filteredCashiers.length} of {cashiers.length} cashiers
				</p>
				<div class="flex items-center gap-4 text-sm text-muted-foreground">
					<span class="flex items-center gap-1">
						<div class="w-2 h-2 rounded-full bg-green-500"></div>
						{filteredCashiers.filter(c => c.email).length} configured
					</span>
					<span class="flex items-center gap-1">
						<div class="w-2 h-2 rounded-full bg-orange-500"></div>
						{filteredCashiers.filter(c => !c.email).length} setup required
					</span>
				</div>
			</div>
		{/if}
	{/if}
</div>