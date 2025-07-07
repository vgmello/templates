<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { Select } from '$lib/components/ui/select';
	import { ArrowLeft, Save, FileText, DollarSign, Calendar, User } from '@lucide/svelte';
	import { invoiceApi, cashierApi, type Invoice, type CreateInvoiceRequest, type GetCashiersResult } from '$lib';
	import { formatDateForInput } from '$lib/utils/date.js';

	let invoiceId = $derived($page.params.id);
	let invoice = $state<Invoice | null>(null);
	let cashiers = $state<GetCashiersResult[]>([]);
	let loading = $state(false);
	let loadingCashiers = $state(false);
	let saving = $state(false);
	let error = $state<string | null>(null);

	let form = $state<CreateInvoiceRequest>({
		name: '',
		amount: 0,
		currency: 'USD',
		dueDate: '',
		cashierId: undefined
	});

	let formErrors = $state<{[key: string]: string}>({});

	// Currency options
	const currencyOptions = [
		{ value: 'USD', label: 'USD - US Dollar' },
		{ value: 'EUR', label: 'EUR - Euro' },
		{ value: 'GBP', label: 'GBP - British Pound' },
		{ value: 'JPY', label: 'JPY - Japanese Yen' },
		{ value: 'CAD', label: 'CAD - Canadian Dollar' },
		{ value: 'AUD', label: 'AUD - Australian Dollar' }
	];

	// Cashier options
	const cashierOptions = $derived([
		{ value: undefined, label: 'No cashier assigned' },
		...cashiers.map(cashier => ({
			value: cashier.cashierId,
			label: cashier.name
		}))
	]);

	function validateForm(): boolean {
		formErrors = {};
		let isValid = true;

		if (!form.name.trim()) {
			formErrors.name = 'Invoice name is required';
			isValid = false;
		}

		if (form.amount <= 0) {
			formErrors.amount = 'Amount must be greater than 0';
			isValid = false;
		}

		if (!form.currency?.trim()) {
			formErrors.currency = 'Currency is required';
			isValid = false;
		}

		// Due date is optional, but if provided, should be valid
		if (form.dueDate && new Date(form.dueDate) < new Date()) {
			formErrors.dueDate = 'Due date cannot be in the past';
			isValid = false;
		}

		return isValid;
	}

	async function loadInvoice() {
		if (!invoiceId) return;
		
		loading = true;
		error = null;
		
		try {
			invoice = await invoiceApi.getInvoice(invoiceId);
			if (invoice) {
				// Populate form with existing data
				form = {
					name: invoice.name,
					amount: invoice.amount,
					currency: invoice.currency || 'USD',
					dueDate: invoice.dueDate ? invoice.dueDate.split('T')[0] : '',
					cashierId: invoice.cashierId || undefined
				};
			}
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load invoice';
			console.error('Error loading invoice:', err);
		} finally {
			loading = false;
		}
	}

	async function loadCashiers() {
		loadingCashiers = true;
		try {
			cashiers = await cashierApi.getCashiers();
		} catch (err) {
			console.error('Failed to load cashiers:', err);
		} finally {
			loadingCashiers = false;
		}
	}

	async function saveInvoice() {
		// Note: This is a placeholder since the current API doesn't have an update endpoint
		// In a real implementation, you would have an update endpoint
		alert('Edit functionality is not yet implemented in the API. This would update the invoice with the new data.');
	}

	function goBack() {
		goto(`/invoices/${invoiceId}`);
	}

	function cancelEdit() {
		goto(`/invoices/${invoiceId}`);
	}

	onMount(() => {
		loadInvoice();
		loadCashiers();
	});
</script>

<svelte:head>
	<title>Edit {invoice?.name || 'Invoice'} - Billing System</title>
</svelte:head>

<div class="container mx-auto p-6 space-y-8">
	<!-- Header with back navigation -->
	<div class="flex items-center justify-between">
		<div class="flex items-center gap-4">
			<Button variant="ghost" onclick={goBack} class="gap-2">
				<ArrowLeft size={16} />
				Back to Invoice
			</Button>
			<div class="h-6 w-px bg-border"></div>
			<div class="flex items-center gap-2">
				<FileText size={20} class="text-primary" />
				<h1 class="text-2xl font-bold">Edit Invoice</h1>
			</div>
		</div>
	</div>

	{#if loading}
		<div class="flex flex-col items-center justify-center py-24 space-y-4">
			<div class="animate-spin rounded-full h-12 w-12 border-2 border-primary border-t-transparent"></div>
			<div class="text-center space-y-1">
				<p class="font-medium">Loading invoice</p>
				<p class="text-sm text-muted-foreground">Please wait while we fetch the invoice details...</p>
			</div>
		</div>
	{:else if error}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="p-4">
				<div class="flex items-center gap-2 text-destructive">
					<div class="font-medium">Error loading invoice</div>
				</div>
				<p class="text-sm text-destructive/80 mt-1">{error}</p>
			</CardContent>
		</Card>
	{:else if invoice}
		<!-- Notice about API limitation -->
		<Card class="border-yellow-200 bg-yellow-50">
			<CardContent class="p-4">
				<div class="flex items-center gap-2 text-yellow-800">
					<div class="font-medium">Note</div>
				</div>
				<p class="text-sm text-yellow-700 mt-1">
					The edit functionality is currently limited as the backend API doesn't have an update endpoint. 
					This is a demonstration of the UI that would be used for editing invoices.
				</p>
			</CardContent>
		</Card>

		<div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
			<!-- Main Form -->
			<div class="lg:col-span-2">
				<Card>
					<CardHeader>
						<CardTitle>Edit Invoice Details</CardTitle>
						<p class="text-sm text-muted-foreground">
							Update the information for this invoice
						</p>
					</CardHeader>
					<CardContent class="space-y-6">
						<!-- Invoice Name -->
						<div class="space-y-2">
							<label for="name" class="text-sm font-medium flex items-center gap-2">
								<FileText size={14} />
								Invoice Name *
							</label>
							<Input
								id="name"
								bind:value={form.name}
								placeholder="Enter invoice description"
								class={formErrors.name ? 'border-destructive' : ''}
							/>
							{#if formErrors.name}
								<p class="text-sm text-destructive">{formErrors.name}</p>
							{/if}
						</div>

						<!-- Amount and Currency -->
						<div class="grid grid-cols-1 md:grid-cols-2 gap-4">
							<div class="space-y-2">
								<label for="amount" class="text-sm font-medium flex items-center gap-2">
									<DollarSign size={14} />
									Amount *
								</label>
								<Input
									id="amount"
									type="number"
									step="0.01"
									min="0"
									bind:value={form.amount}
									placeholder="0.00"
									class={formErrors.amount ? 'border-destructive' : ''}
								/>
								{#if formErrors.amount}
									<p class="text-sm text-destructive">{formErrors.amount}</p>
								{/if}
							</div>

							<div class="space-y-2">
								<label for="currency" class="text-sm font-medium">Currency *</label>
								<Select
									id="currency"
									bind:value={form.currency}
									options={currencyOptions}
									placeholder="Select currency"
									error={formErrors.currency}
								/>
							</div>
						</div>

						<!-- Due Date -->
						<div class="space-y-2">
							<label for="dueDate" class="text-sm font-medium flex items-center gap-2">
								<Calendar size={14} />
								Due Date (Optional)
							</label>
							<Input
								id="dueDate"
								type="date"
								bind:value={form.dueDate}
								min={formatDateForInput()}
								class={formErrors.dueDate ? 'border-destructive' : ''}
							/>
							{#if formErrors.dueDate}
								<p class="text-sm text-destructive">{formErrors.dueDate}</p>
							{/if}
						</div>

						<!-- Cashier Selection -->
						<div class="space-y-2">
							<label for="cashier" class="text-sm font-medium flex items-center gap-2">
								<User size={14} />
								Assigned Cashier (Optional)
							</label>
							{#if loadingCashiers}
								<div class="text-sm text-muted-foreground">Loading cashiers...</div>
							{:else}
								<Select
									id="cashier"
									bind:value={form.cashierId}
									options={cashierOptions}
									placeholder="Select cashier"
								/>
							{/if}
						</div>
					</CardContent>
				</Card>
			</div>

			<!-- Preview and Actions -->
			<div class="space-y-6">
				<!-- Preview Card -->
				<Card>
					<CardHeader>
						<CardTitle class="text-lg">Invoice Preview</CardTitle>
					</CardHeader>
					<CardContent class="space-y-4">
						<div class="space-y-3">
							<div>
								<label class="text-xs text-muted-foreground">Name</label>
								<p class="font-medium">{form.name || 'Untitled Invoice'}</p>
							</div>
							
							<div>
								<label class="text-xs text-muted-foreground">Amount</label>
								<p class="text-lg font-bold">
									{new Intl.NumberFormat('en-US', {
										style: 'currency',
										currency: form.currency || 'USD'
									}).format(form.amount)}
								</p>
							</div>

							{#if form.dueDate}
								<div>
									<label class="text-xs text-muted-foreground">Due Date</label>
									<p>{new Date(form.dueDate).toLocaleDateString('en-US', {
										weekday: 'long',
										year: 'numeric',
										month: 'long',
										day: 'numeric'
									})}</p>
								</div>
							{/if}

							{#if form.cashierId}
								<div>
									<label class="text-xs text-muted-foreground">Assigned Cashier</label>
									<p>{cashiers.find(c => c.cashierId === form.cashierId)?.name}</p>
								</div>
							{/if}
						</div>
					</CardContent>
				</Card>

				<!-- Actions -->
				<Card>
					<CardContent class="p-4 space-y-3">
						<Button 
							onclick={saveInvoice} 
							disabled={saving || !form.name || form.amount <= 0}
							class="w-full gap-2"
						>
							{#if saving}
								<div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
							{:else}
								<Save size={16} />
							{/if}
							Save Changes
						</Button>
						
						<Button variant="outline" onclick={cancelEdit} disabled={saving} class="w-full">
							Cancel
						</Button>
					</CardContent>
				</Card>
			</div>
		</div>
	{/if}
</div>