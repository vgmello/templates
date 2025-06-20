<script>
	import Button from "$lib/components/ui/button.svelte";
	import Card from "$lib/components/ui/card.svelte";
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

		<div class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
			<Card class="p-6">
				<div class="space-y-2">
					<h3 class="text-xl font-semibold">Cashiers</h3>
					<p class="text-sm text-muted-foreground">
						Manage cashiers and their payment configurations.
					</p>
					<Button class="w-full" onclick="window.location.href='/cashiers'">
						Manage Cashiers
					</Button>
				</div>
			</Card>

			<Card class="p-6">
				<div class="space-y-2">
					<h3 class="text-xl font-semibold">Invoices</h3>
					<p class="text-sm text-muted-foreground">
						Create and track invoices for customers.
					</p>
					<Button variant="outline" class="w-full" disabled>
						Coming Soon
					</Button>
				</div>
			</Card>

			<Card class="p-6">
				<div class="space-y-2">
					<h3 class="text-xl font-semibold">Payments</h3>
					<p class="text-sm text-muted-foreground">
						Track and process customer payments.
					</p>
					<Button variant="outline" class="w-full" disabled>
						Coming Soon
					</Button>
				</div>
			</Card>
		</div>
	</div>
</div>