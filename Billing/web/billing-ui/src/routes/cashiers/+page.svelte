<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '$lib/components/ui/table';
	import { Input } from '$lib/components/ui/input';
	import { Plus, Search, Pencil, Trash2 } from '@lucide/svelte';
	import { cashierApi, type GetCashiersResult } from '$lib';

	let cashiers = $state<GetCashiersResult[]>([]);
	let loading = $state(false);
	let error = $state<string | null>(null);
	let searchTerm = $state('');

	// Reactive filtered cashiers
	let filteredCashiers = $derived(
		cashiers.filter(cashier => 
			cashier.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
			cashier.email.toLowerCase().includes(searchTerm.toLowerCase())
		)
	);

	async function loadCashiers() {
		loading = true;
		error = null;
		
		try {
			cashiers = await cashierApi.getCashiers();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load cashiers';
			console.error('Error loading cashiers:', err);
		} finally {
			loading = false;
		}
	}

	async function deleteCashier(id: string, name: string) {
		if (!confirm(`Are you sure you want to delete cashier "${name}"?`)) {
			return;
		}

		try {
			await cashierApi.deleteCashier(id);
			await loadCashiers(); // Reload the list
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

	onMount(() => {
		loadCashiers();
	});
</script>

<svelte:head>
	<title>Cashier Management - Billing System</title>
</svelte:head>

<div class="container mx-auto p-6 space-y-6">
	<div class="flex items-center justify-between">
		<div>
			<h1 class="text-3xl font-bold tracking-tight">Cashier Management</h1>
			<p class="text-muted-foreground">Manage cashiers and their information</p>
		</div>
		<Button onclick={createCashier} class="flex items-center gap-2">
			<Plus size={16} />
			Add Cashier
		</Button>
	</div>

	<Card>
		<CardHeader>
			<CardTitle>Cashiers</CardTitle>
			<div class="flex items-center space-x-2">
				<Search size={16} class="text-muted-foreground" />
				<Input
					bind:value={searchTerm}
					placeholder="Search cashiers by name or email..."
					class="max-w-sm"
				/>
			</div>
		</CardHeader>
		<CardContent>
			{#if loading}
				<div class="flex items-center justify-center py-8">
					<div class="text-center">
						<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
						<p class="mt-2 text-sm text-muted-foreground">Loading cashiers...</p>
					</div>
				</div>
			{:else if error}
				<div class="text-center py-8">
					<p class="text-destructive mb-4">{error}</p>
					<Button onclick={loadCashiers} variant="outline">
						Try Again
					</Button>
				</div>
			{:else if filteredCashiers.length === 0}
				<div class="text-center py-8">
					<p class="text-muted-foreground mb-4">
						{searchTerm ? 'No cashiers found matching your search.' : 'No cashiers found.'}
					</p>
					{#if !searchTerm}
						<Button onclick={createCashier}>
							<Plus size={16} class="mr-2" />
							Add Your First Cashier
						</Button>
					{/if}
				</div>
			{:else}
				<Table>
					<TableHeader>
						<TableRow>
							<TableHead>Name</TableHead>
							<TableHead>Email</TableHead>
							<TableHead>ID</TableHead>
							<TableHead class="text-right">Actions</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{#each filteredCashiers as cashier}
							<TableRow>
								<TableCell class="font-medium">{cashier.name}</TableCell>
								<TableCell>{cashier.email || 'N/A'}</TableCell>
								<TableCell class="font-mono text-sm text-muted-foreground">
									{cashier.cashierId}
								</TableCell>
								<TableCell class="text-right">
									<div class="flex items-center justify-end gap-2">
										<Button
											size="sm"
											variant="outline"
											onclick={() => editCashier(cashier.cashierId)}
										>
											<Pencil size={14} />
										</Button>
										<Button
											size="sm"
											variant="destructive"
											onclick={() => deleteCashier(cashier.cashierId, cashier.name)}
										>
											<Trash2 size={14} />
										</Button>
									</div>
								</TableCell>
							</TableRow>
						{/each}
					</TableBody>
				</Table>
			{/if}
		</CardContent>
	</Card>
</div>