<script lang="ts">
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { enhance } from '$app/forms';
	import Button from "$lib/components/ui/button.svelte";
	import Card from "$lib/components/ui/card.svelte";
	import Badge from "$lib/components/ui/badge.svelte";
	import Label from "$lib/components/ui/label.svelte";
	import { cashierStore } from '$lib/stores/cashier.svelte';
	import { ArrowLeft, User, Mail, CreditCard, Calendar, Edit, Trash2, AlertCircle } from "lucide-svelte";
	import type { ActionResult } from '@sveltejs/kit';

	let { data } = $props();
	
	// Initialize store with SSR data
	onMount(() => {
		cashierStore.initializeSelectedCashier(data.cashier);
	});
	
	// Get cashier from data directly (since it's server-side loaded)
	let cashier = data.cashier;
	// Direct access to store error
	
	// Form references and state
	let deleteFormRef: HTMLFormElement | undefined = $state();
	let deleteError = $state(null);
	let deleting = $state(false);

	function handleGoBack() {
		goto('/cashiers');
	}

	function handleEditCashier() {
		if (cashier) {
			goto(`/cashiers/${cashier.cashierId}/edit`);
		}
	}


	function handleDeleteCashier() {
		if (!cashier || !confirm('Are you sure you want to delete this cashier? This action cannot be undone.')) {
			return;
		}
		
		// Submit the delete form using bind:this reference
		if (deleteFormRef) {
			deleteFormRef.requestSubmit();
		}
	}

	function handleDeleteEnhance() {
		deleting = true;
		deleteError = null;
		
		return async ({ result, update }: { result: ActionResult; update: () => Promise<void> }) => {
			deleting = false;
			
			if (result.type === 'error') {
				deleteError = result.error?.message || 'Failed to delete cashier';
			}
			
			await update();
		};
	}

	function formatDate(dateString: string | undefined): string {
		if (!dateString) return 'Unknown';
		return new Date(dateString).toLocaleString();
	}

	function dismissError() {
		deleteError = null;
		cashierStore.clearError();
	}
</script>

<svelte:head>
	<title>{cashier?.name || 'Cashier Details'} - Billing Service</title>
</svelte:head>

<div class="container mx-auto px-4 py-8">
	<div class="max-w-4xl mx-auto space-y-6">
		<!-- Error Banner -->
		{#if deleteError || cashierStore.error}
			<Card class="p-4 border-destructive bg-destructive/10">
				<div class="flex items-center gap-3">
					<AlertCircle class="h-5 w-5 text-destructive" />
					<div class="flex-1">
						<p class="text-sm font-medium text-destructive">
							{deleteError ? 'Error deleting cashier' : 'Error loading cashier'}
						</p>
						<p class="text-sm text-destructive/80">{deleteError || cashierStore.error}</p>
					</div>
					<Button onclick={dismissError} variant="ghost" size="sm">
						Dismiss
					</Button>
				</div>
			</Card>
		{/if}

		<!-- Header -->
		<div class="flex items-center gap-4">
			<Button variant="ghost" size="icon" onclick={handleGoBack}>
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
					<Button variant="outline" onclick={handleEditCashier} class="gap-2">
						<Edit class="h-4 w-4" />
						Edit
					</Button>
					<Button variant="destructive" onclick={handleDeleteCashier} class="gap-2" disabled={deleting}>
						<Trash2 class="h-4 w-4" />
						{deleting ? 'Deleting...' : 'Delete'}
					</Button>
				</div>
			{/if}
		</div>

		<!-- Cashier Details -->
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
								<Button variant="outline" size="sm" onclick={handleEditCashier}>
									Configure Payments
								</Button>
							</div>
						{/if}
					</div>
				</Card>
			</div>

		<!-- Hidden delete form -->
		<form bind:this={deleteFormRef} method="POST" action="?/delete" use:enhance={handleDeleteEnhance} style="display: none;"></form>
	</div>
</div>