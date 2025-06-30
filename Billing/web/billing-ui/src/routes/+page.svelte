<script lang="ts">
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Users, CreditCard, BarChart3 } from '@lucide/svelte';
</script>

<svelte:head>
	<title>Billing System - Home</title>
</svelte:head>

<div class="container mx-auto p-6 space-y-8">
	<div class="text-center space-y-4">
		<h1 class="text-4xl font-bold tracking-tight">Welcome to Billing System</h1>
		<p class="text-xl text-muted-foreground max-w-2xl mx-auto">
			Manage your cashiers, invoices, and billing operations with ease
		</p>
	</div>

	<div class="grid grid-cols-1 md:grid-cols-3 gap-6">
		<Card>
			<CardHeader class="text-center">
				<Users size={48} class="mx-auto mb-4 text-primary" />
				<CardTitle>Cashier Management</CardTitle>
			</CardHeader>
			<CardContent class="text-center space-y-4">
				<p class="text-muted-foreground">
					Add, edit, and manage cashiers in your system
				</p>
				<Button href="/cashiers" class="w-full">
					Manage Cashiers
				</Button>
			</CardContent>
		</Card>

		<Card>
			<CardHeader class="text-center">
				<CreditCard size={48} class="mx-auto mb-4 text-primary" />
				<CardTitle>Invoices</CardTitle>
			</CardHeader>
			<CardContent class="text-center space-y-4">
				<p class="text-muted-foreground">
					Create and manage invoices for your customers
				</p>
				<Button href="/invoices" class="w-full">
					Manage Invoices
				</Button>
			</CardContent>
		</Card>

		<Card>
			<CardHeader class="text-center">
				<BarChart3 size={48} class="mx-auto mb-4 text-primary" />
				<CardTitle>Reports</CardTitle>
			</CardHeader>
			<CardContent class="text-center space-y-4">
				<p class="text-muted-foreground">
					View billing reports and analytics
				</p>
				<Button variant="outline" class="w-full" disabled>
					Coming Soon
				</Button>
			</CardContent>
		</Card>
	</div>

	<div class="text-center">
		<p class="text-sm text-muted-foreground">
			Some placeholder
		</p>
	</div>
</div>
