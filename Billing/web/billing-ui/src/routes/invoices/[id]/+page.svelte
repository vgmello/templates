<script lang="ts">
	import { enhance } from '$app/forms';
	import { goto, invalidateAll } from '$app/navigation';
	import Button from '$lib/components/ui/button.svelte';
	import Card from '$lib/components/ui/card.svelte';
	import Input from '$lib/components/ui/input.svelte';
	import Badge from '$lib/components/ui/badge.svelte';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();

	let isSubmitting = $state(false);
	let showPaymentForm = $state(false);
	let showSimulateForm = $state(false);

	// Payment form state
	let amountPaid = $state(data.invoice.amount.toString());
	let paymentDate = $state(new Date().toISOString().split('T')[0]);

	// Simulate payment form state
	let simulateAmount = $state(data.invoice.amount.toString());
	let simulateCurrency = $state(data.invoice.currency);
	let paymentMethod = $state('Credit Card');

	// Format currency
	function formatCurrency(amount: number, currency: string): string {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: currency || 'USD'
		}).format(amount);
	}

	// Format date
	function formatDate(dateString: string): string {
		return new Date(dateString).toLocaleDateString('en-US', {
			year: 'numeric',
			month: 'long',
			day: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}

	// Get status badge variant
	function getStatusVariant(status: string): 'default' | 'secondary' | 'destructive' | 'outline' {
		switch (status) {
			case 'Paid': return 'default';
			case 'Draft': return 'secondary';
			case 'Cancelled': return 'destructive';
			default: return 'outline';
		}
	}

	// Check if invoice is overdue
	function isOverdue(invoice: any): boolean {
		if (!invoice.dueDate || invoice.status === 'Paid' || invoice.status === 'Cancelled') return false;
		return new Date(invoice.dueDate) < new Date();
	}

    function handleGoBack() {
		goto('/invoices');
	}

    function goToInvoice(){
        goto(`/invoices/${data.invoice.invoiceId}/edit`)
    }

	// Can perform actions
	let canCancel = $derived(data.invoice.status === 'Draft');
	let canMarkPaid = $derived(data.invoice.status === 'Draft');
	let canSimulate = $derived(data.invoice.status === 'Draft');
	let canEdit = $derived(data.invoice.status === 'Draft');
</script>

<svelte:head>
	<title>{data.invoice.name} - Invoice Details</title>
	<meta name="description" content="Invoice details for {data.invoice.name}" />
</svelte:head>

<div class="container mx-auto px-4 py-8 max-w-4xl">
	<!-- Header -->
	<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between mb-8">
		<div>
			<div class="flex items-center gap-3 mb-2">
				<h1 class="text-3xl font-bold tracking-tight">{data.invoice.name}</h1>
				<Badge class={getStatusVariant(data.invoice.status) === 'destructive' ? 'bg-red-100 text-red-800' : getStatusVariant(data.invoice.status) === 'default' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}>
					{data.invoice.status}
				</Badge>
				{#if isOverdue(data.invoice)}
					<Badge class="bg-red-100 text-red-800">Overdue</Badge>
				{/if}
			</div>
			<p class="text-muted-foreground">
				Invoice ID: {data.invoice.invoiceId}
			</p>
		</div>
		<div class="flex gap-2">
			<Button variant="outline" onclick={handleGoBack}>
				← Back to Invoices
			</Button>
			{#if canEdit}
				<Button variant="outline" onclick={goToInvoice} >
					Edit
				</Button>
			{/if}
		</div>
	</div>

	<!-- Success/Error Messages -->
	{#if form?.success}
		<div class="bg-green-50 text-green-800 px-4 py-3 rounded-md mb-6">
			{form.message}
		</div>
	{/if}

	{#if form?.error}
		<div class="bg-destructive/15 text-destructive px-4 py-3 rounded-md mb-6">
			{form.error}
		</div>
	{/if}

	<div class="grid gap-6 lg:grid-cols-2">
		<!-- Invoice Details -->
		<Card class="p-6">
			<h2 class="text-lg font-semibold mb-4">Invoice Information</h2>
			<div class="space-y-4">
				<div>
					<span class="text-sm font-medium text-muted-foreground">Amount</span>
					<p class="text-2xl font-bold">{formatCurrency(data.invoice.amount, data.invoice.currency)}</p>
				</div>

				<div class="grid grid-cols-2 gap-4">
					<div>
						<span class="text-sm font-medium text-muted-foreground">Currency</span>
						<p>{data.invoice.currency}</p>
					</div>
					<div>
						<span class="text-sm font-medium text-muted-foreground">Status</span>
						<p>{data.invoice.status}</p>
					</div>
				</div>

				{#if data.invoice.dueDate}
					<div>
						<span class="text-sm font-medium text-muted-foreground">Due Date</span>
						<p class:text-red-600={isOverdue(data.invoice)}>
							{formatDate(data.invoice.dueDate)}
							{#if isOverdue(data.invoice)}
								(Overdue)
							{/if}
						</p>
					</div>
				{/if}

				{#if data.invoice.cashierId}
					<div>
						<span class="text-sm font-medium text-muted-foreground">Assigned Cashier</span>
						<p>{data.invoice.cashierId}</p>
					</div>
				{/if}

				<div class="grid grid-cols-2 gap-4 pt-4 border-t">
					<div>
						<span class="text-sm font-medium text-muted-foreground">Created</span>
						<p class="text-sm">{formatDate(data.invoice.createdDateUtc)}</p>
					</div>
					<div>
						<span class="text-sm font-medium text-muted-foreground">Updated</span>
						<p class="text-sm">{formatDate(data.invoice.updatedDateUtc)}</p>
					</div>
				</div>
			</div>
		</Card>

		<!-- Actions -->
		<Card class="p-6">
			<div class="mb-4">
				<h2 class="text-lg font-semibold">Actions</h2>
				<p class="text-sm text-muted-foreground">Manage this invoice</p>
			</div>
			<div class="space-y-4">
				<!-- Mark as Paid -->
				{#if canMarkPaid}
					<div class="space-y-3">
						<Button
							onclick={() => showPaymentForm = !showPaymentForm}
							class="w-full"
							variant={showPaymentForm ? 'outline' : 'default'}
						>
							{showPaymentForm ? 'Cancel Payment' : 'Mark as Paid'}
						</Button>

						{#if showPaymentForm}
							<form
								method="POST"
								action="?/markPaid"
								use:enhance={() => {
									isSubmitting = true;
									return async ({ result, update }) => {
										await update();
										isSubmitting = false;

										// Only close form and refresh data on success
										if (result.type === 'success' && result.data?.success) {
											showPaymentForm = false;
											await invalidateAll();
										}
									};
								}}
								class="space-y-3 p-4 border rounded-lg"
							>
								<div>
									<label for="amountPaid" class="text-sm font-medium">Amount Paid</label>
									<Input
										id="amountPaid"
										name="amountPaid"
										type="number"
										step="0.01"
										bind:value={amountPaid}
										required
										disabled={isSubmitting}
									/>
								</div>
								<div>
									<label for="paymentDate" class="text-sm font-medium">Payment Date</label>
									<Input
										id="paymentDate"
										name="paymentDate"
										type="date"
										bind:value={paymentDate}
										disabled={isSubmitting}
									/>
								</div>
								<Button type="submit" disabled={isSubmitting} class="w-full">
									{isSubmitting ? 'Processing...' : 'Confirm Payment'}
								</Button>
							</form>
						{/if}
					</div>
				{/if}

				<!-- Simulate Payment -->
				{#if canSimulate}
					<div class="space-y-3">
						<Button
							onclick={() => showSimulateForm = !showSimulateForm}
							variant="outline"
							class="w-full"
						>
							{showSimulateForm ? 'Cancel Simulation' : 'Simulate Payment'}
						</Button>

						{#if showSimulateForm}
							<form
								method="POST"
								action="?/simulatePayment"
								use:enhance={() => {
									isSubmitting = true;
									return async ({ result, update }) => {
										await update();
										isSubmitting = false;

										// Only close form on success
										if (result.type === 'success' && result.data?.success) {
											showSimulateForm = false;
										}
									};
								}}
								class="space-y-3 p-4 border rounded-lg"
							>
								<div>
									<label for="amount" class="text-sm font-medium">Amount</label>
									<Input
										id="amount"
										name="amount"
										type="number"
										step="0.01"
										bind:value={simulateAmount}
										required
										disabled={isSubmitting}
									/>
								</div>
								<div>
									<label for="currency" class="text-sm font-medium">Currency</label>
									<Input
										id="currency"
										name="currency"
										bind:value={simulateCurrency}
										disabled={isSubmitting}
									/>
								</div>
								<div>
									<label for="paymentMethod" class="text-sm font-medium">Payment Method</label>
									<select
										id="paymentMethod"
										name="paymentMethod"
										bind:value={paymentMethod}
										disabled={isSubmitting}
										class="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
									>
										<option value="Credit Card">Credit Card</option>
										<option value="Bank Transfer">Bank Transfer</option>
										<option value="PayPal">PayPal</option>
										<option value="Cash">Cash</option>
									</select>
								</div>
								<Button type="submit" disabled={isSubmitting} class="w-full">
									{isSubmitting ? 'Simulating...' : 'Simulate Payment'}
								</Button>
							</form>
						{/if}
					</div>
				{/if}

				<!-- Cancel Invoice -->
				{#if canCancel}
					<form
						method="POST"
						action="?/cancel"
						use:enhance={() => {
							isSubmitting = true;
							return async ({ result, update }) => {
								await update();
								isSubmitting = false;

								// Only refresh data on success
								if (result.type === 'success' && result.data?.success) {
									await invalidateAll();
								}
							};
						}}
					>
						<Button
							type="submit"
							variant="destructive"
							disabled={isSubmitting}
							class="w-full"
							onclick={(e) => {
								if (!confirm('Are you sure you want to cancel this invoice? This action cannot be undone.')) {
									e.preventDefault();
								}
							}}
						>
							{isSubmitting ? 'Cancelling...' : 'Cancel Invoice'}
						</Button>
					</form>
				{/if}

				{#if data.invoice.status === 'Paid'}
					<div class="text-center text-sm text-muted-foreground p-4 bg-green-50 rounded-lg">
						This invoice has been paid and cannot be modified.
					</div>
				{/if}

				{#if data.invoice.status === 'Cancelled'}
					<div class="text-center text-sm text-muted-foreground p-4 bg-gray-50 rounded-lg">
						This invoice has been cancelled and cannot be modified.
					</div>
				{/if}
			</div>
		</Card>
	</div>
</div>
