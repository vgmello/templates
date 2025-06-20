<script lang="ts">
	import { goto } from '$app/navigation';
	import Button from "$lib/components/ui/button.svelte";
	import Card from "$lib/components/ui/card.svelte";
	import { Users, FileText, CreditCard, ArrowRight } from 'lucide-svelte';

	function navigateToCashiers() {
		goto('/cashiers');
	}

	// Keyboard navigation support
	function handleKeydown(event: KeyboardEvent, callback: () => void) {
		if (event.key === 'Enter' || event.key === ' ') {
			event.preventDefault();
			callback();
		}
	}
</script>

<svelte:head>
	<title>Billing Service</title>
	<meta name="description" content="Billing Service Management Dashboard" />
</svelte:head>

<div class="container mx-auto px-4 py-8">
	<div class="space-y-6">
		<div class="space-y-2">
			<h1 class="text-3xl font-bold tracking-tight">Billing Service</h1>
			<p class="text-muted-foreground">
				Manage cashiers, invoices, and payments for your business.
			</p>
		</div>

		<div class="grid gap-6 md:grid-cols-2 lg:grid-cols-3" role="grid" aria-label="Service modules">
			<Card 
				class="p-6 hover:shadow-lg transition-shadow cursor-pointer focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2" 
				role="gridcell"
				tabindex={0}
				onkeydown={(e) => handleKeydown(e, navigateToCashiers)}
				onclick={navigateToCashiers}
				aria-label="Navigate to Cashiers management"
			>
				<div class="space-y-4">
					<div class="flex items-center gap-3">
						<div class="flex h-10 w-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
							<Users class="h-5 w-5" aria-hidden="true" />
						</div>
						<h3 class="text-xl font-semibold">Cashiers</h3>
					</div>
					<p class="text-sm text-muted-foreground">
						Manage cashiers and their payment configurations.
					</p>
					<div class="flex items-center text-primary text-sm font-medium">
						<span>Manage Cashiers</span>
						<ArrowRight class="ml-2 h-4 w-4" aria-hidden="true" />
					</div>
				</div>
			</Card>

			<Card class="p-6 opacity-60" role="gridcell" aria-label="Invoices module - Coming soon">
				<div class="space-y-4">
					<div class="flex items-center gap-3">
						<div class="flex h-10 w-10 items-center justify-center rounded-lg bg-muted text-muted-foreground">
							<FileText class="h-5 w-5" aria-hidden="true" />
						</div>
						<h3 class="text-xl font-semibold">Invoices</h3>
					</div>
					<p class="text-sm text-muted-foreground">
						Create and track invoices for customers.
					</p>
					<div class="flex items-center text-muted-foreground text-sm font-medium">
						<span>Coming Soon</span>
					</div>
				</div>
			</Card>

			<Card class="p-6 opacity-60" role="gridcell" aria-label="Payments module - Coming soon">
				<div class="space-y-4">
					<div class="flex items-center gap-3">
						<div class="flex h-10 w-10 items-center justify-center rounded-lg bg-muted text-muted-foreground">
							<CreditCard class="h-5 w-5" aria-hidden="true" />
						</div>
						<h3 class="text-xl font-semibold">Payments</h3>
					</div>
					<p class="text-sm text-muted-foreground">
						Track and process customer payments.
					</p>
					<div class="flex items-center text-muted-foreground text-sm font-medium">
						<span>Coming Soon</span>
					</div>
				</div>
			</Card>
		</div>
	</div>
</div>