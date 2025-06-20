<script>
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import Button from "$lib/components/ui/button.svelte";
	import Card from "$lib/components/ui/card.svelte";
	import Badge from "$lib/components/ui/badge.svelte";
	import Label from "$lib/components/ui/label.svelte";
	import { cashierService } from "$lib/api.js";
	import { ArrowLeft, User, Mail, CreditCard, Calendar, Edit, Trash2 } from "lucide-svelte";

	let cashier = $state(null);
	let loading = $state(true);
	let error = $state(null);
	
	$: cashierId = $page.params.id;

	onMount(async () => {
		await loadCashier();
	});

	async function loadCashier() {
		try {
			loading = true;
			error = null;
			cashier = await cashierService.getCashier(cashierId);
		} catch (err) {
			error = err.message;
			console.error('Failed to load cashier:', err);
		} finally {
			loading = false;
		}
	}

	function goBack() {
		window.location.href = '/cashiers';
	}

	function editCashier() {
		window.location.href = `/cashiers/${cashierId}/edit`;
	}

	async function deleteCashier() {
		if (!confirm('Are you sure you want to delete this cashier? This action cannot be undone.')) {
			return;
		}

		try {
			await cashierService.deleteCashier(cashierId);
			window.location.href = '/cashiers';
		} catch (err) {
			error = err.message;
			console.error('Failed to delete cashier:', err);
		}
	}

	function formatDate(dateString) {
		if (!dateString) return 'Unknown';
		return new Date(dateString).toLocaleString();
	}
</script>

<svelte:head>
	<title>{cashier?.name || 'Cashier Details'} - Billing Service</title>
</svelte:head>

<div class="container mx-auto px-4 py-8">
	<div class="max-w-4xl mx-auto space-y-6">
		<!-- Header -->
		<div class="flex items-center gap-4">
			<Button variant="ghost" size="icon" onclick={goBack}>
				<ArrowLeft class="h-4 w-4" />
			</Button>
			<div class="flex-1">
				<h1 class="text-3xl font-bold tracking-tight">Cashier Details</h1>
				<p class="text-muted-foreground">
					View and manage cashier information.
				</p>
			</div>
			{#if cashier}
				<div class="flex gap-2">
					<Button variant="outline" onclick={editCashier} class="gap-2">
						<Edit class="h-4 w-4" />
						Edit
					</Button>
					<Button variant="destructive" onclick={deleteCashier} class="gap-2">
						<Trash2 class="h-4 w-4" />
						Delete
					</Button>
				</div>
			{/if}
		</div>

		<!-- Loading State -->
		{#if loading}
			<div class="flex items-center justify-center py-12">
				<div class="text-center space-y-2">
					<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
					<p class="text-sm text-muted-foreground">Loading cashier details...</p>
				</div>
			</div>
		{/if}

		<!-- Error State -->
		{#if error}
			<Card class="p-6">
				<div class="text-center space-y-2">
					<p class="text-destructive font-medium">Error loading cashier</p>
					<p class="text-sm text-muted-foreground">{error}</p>
					<Button variant="outline" onclick={loadCashier} size="sm">
						Try Again
					</Button>
				</div>
			</Card>
		{/if}

		<!-- Cashier Details -->
		{#if cashier && !loading}
			<div class="grid gap-6 md:grid-cols-2">
				<!-- Basic Information -->
				<Card class="p-6">
					<div class="space-y-4">
						<h3 class="text-lg font-semibold flex items-center gap-2">
							<User class="h-5 w-5" />
							Basic Information
						</h3>
						
						<div class="space-y-3">
							<div>
								<Label class="text-sm font-medium text-muted-foreground">Name</Label>
								<p class="text-sm font-medium">{cashier.name}</p>
							</div>
							
							<div>
								<Label class="text-sm font-medium text-muted-foreground">Email</Label>
								<p class="text-sm flex items-center gap-2">
									<Mail class="h-3 w-3" />
									{cashier.email}
								</p>
							</div>
							
							<div>
								<Label class="text-sm font-medium text-muted-foreground">Cashier ID</Label>
								<p class="text-sm font-mono text-muted-foreground">{cashier.cashierId}</p>
							</div>
						</div>
					</div>
				</Card>

				<!-- Timestamps -->
				<Card class="p-6">
					<div class="space-y-4">
						<h3 class="text-lg font-semibold flex items-center gap-2">
							<Calendar class="h-5 w-5" />
							Timestamps
						</h3>
						
						<div class="space-y-3">
							<div>
								<Label class="text-sm font-medium text-muted-foreground">Created</Label>
								<p class="text-sm">{formatDate(cashier.createdDateUtc)}</p>
							</div>
							
							<div>
								<Label class="text-sm font-medium text-muted-foreground">Last Updated</Label>
								<p class="text-sm">{formatDate(cashier.updatedDateUtc)}</p>
							</div>
							
							{#if cashier.version}
								<div>
									<Label class="text-sm font-medium text-muted-foreground">Version</Label>
									<p class="text-sm">{cashier.version}</p>
								</div>
							{/if}
						</div>
					</div>
				</Card>

				<!-- Payment Configurations -->
				<Card class="p-6 md:col-span-2">
					<div class="space-y-4">
						<h3 class="text-lg font-semibold flex items-center gap-2">
							<CreditCard class="h-5 w-5" />
							Payment Configurations
						</h3>
						
						{#if cashier.cashierPayments && cashier.cashierPayments.length > 0}
							<div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
								{#each cashier.cashierPayments as payment}
									<div class="border rounded-lg p-4 space-y-2">
										<div class="flex items-center justify-between">
											<Badge variant="default">{payment.currency}</Badge>
											{#if payment.isActive}
												<Badge variant="secondary" class="text-xs">Active</Badge>
											{:else}
												<Badge variant="outline" class="text-xs">Inactive</Badge>
											{/if}
										</div>
										
										{#if payment.description}
											<p class="text-sm text-muted-foreground">{payment.description}</p>
										{/if}
										
										{#if payment.createdDateUtc}
											<p class="text-xs text-muted-foreground">
												Added: {formatDate(payment.createdDateUtc)}
											</p>
										{/if}
									</div>
								{/each}
							</div>
						{:else}
							<div class="text-center py-8 space-y-2">
								<CreditCard class="h-12 w-12 mx-auto text-muted-foreground" />
								<h4 class="font-medium">No payment configurations</h4>
								<p class="text-sm text-muted-foreground">
									This cashier doesn't have any payment methods configured yet.
								</p>
								<Button variant="outline" size="sm" onclick={editCashier}>
									Configure Payments
								</Button>
							</div>
						{/if}
					</div>
				</Card>
			</div>
		{/if}
	</div>
</div>