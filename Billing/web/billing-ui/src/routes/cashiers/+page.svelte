<script>
	import { onMount } from 'svelte';
	import Button from "$lib/components/ui/button.svelte";
	import Card from "$lib/components/ui/card.svelte";
	import Badge from "$lib/components/ui/badge.svelte";
	import { cashierService } from "$lib/api.js";
	import { Plus, User, Mail, CreditCard } from "lucide-svelte";

	let cashiers = $state([]);
	let loading = $state(true);
	let error = $state(null);

	onMount(async () => {
		await loadCashiers();
	});

	async function loadCashiers() {
		try {
			loading = true;
			error = null;
			cashiers = await cashierService.getCashiers();
		} catch (err) {
			error = err.message;
			console.error('Failed to load cashiers:', err);
		} finally {
			loading = false;
		}
	}

	function goToCreateCashier() {
		window.location.href = '/cashiers/create';
	}

	function goToCashierDetails(cashierId) {
		window.location.href = `/cashiers/${cashierId}`;
	}
</script>

<svelte:head>
	<title>Cashiers - Billing Service</title>
</svelte:head>

<div class="container mx-auto px-4 py-8">
	<div class="space-y-6">
		<!-- Header -->
		<div class="flex items-center justify-between">
			<div class="space-y-1">
				<h1 class="text-3xl font-bold tracking-tight">Cashiers</h1>
				<p class="text-muted-foreground">
					Manage cashiers and their payment configurations.
				</p>
			</div>
			<Button onclick={goToCreateCashier} class="gap-2">
				<Plus class="h-4 w-4" />
				Add Cashier
			</Button>
		</div>

		<!-- Loading State -->
		{#if loading}
			<div class="flex items-center justify-center py-12">
				<div class="text-center space-y-2">
					<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
					<p class="text-sm text-muted-foreground">Loading cashiers...</p>
				</div>
			</div>
		{/if}

		<!-- Error State -->
		{#if error}
			<Card class="p-6">
				<div class="text-center space-y-2">
					<p class="text-destructive font-medium">Error loading cashiers</p>
					<p class="text-sm text-muted-foreground">{error}</p>
					<Button variant="outline" onclick={loadCashiers} size="sm">
						Try Again
					</Button>
				</div>
			</Card>
		{/if}

		<!-- Cashiers Grid -->
		{#if !loading && !error}
			{#if cashiers.length === 0}
				<Card class="p-12">
					<div class="text-center space-y-4">
						<User class="h-12 w-12 mx-auto text-muted-foreground" />
						<div class="space-y-2">
							<h3 class="text-lg font-semibold">No cashiers found</h3>
							<p class="text-sm text-muted-foreground max-w-sm mx-auto">
								Get started by creating your first cashier to handle payments.
							</p>
						</div>
						<Button onclick={goToCreateCashier} class="gap-2">
							<Plus class="h-4 w-4" />
							Create First Cashier
						</Button>
					</div>
				</Card>
			{:else}
				<div class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
					{#each cashiers as cashier (cashier.cashierId)}
						<Card class="p-6 hover:shadow-md transition-shadow cursor-pointer" onclick={() => goToCashierDetails(cashier.cashierId)}>
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
		{/if}
	</div>
</div>