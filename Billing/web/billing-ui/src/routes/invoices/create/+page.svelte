<script lang="ts">
	import { goto } from '$app/navigation';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { CurrencyInput } from '$lib/components/ui/currency-input';
	import { Select } from '$lib/components/ui/select';
	import { ArrowLeft, Save, FileText, DollarSign, Calendar, User } from '@lucide/svelte';
	import { invoiceApi, type CreateInvoiceRequest, type GetCashiersResult } from '$lib';
	import { formatDateForInput } from '$lib/utils/date.js';

	type Props = {
		data: {
			cashiers: GetCashiersResult[];
		};
	};
	
	let { data }: Props = $props();
	let { cashiers } = data;
	let loading = $state(false);
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


	async function createInvoice() {
		if (!validateForm() || loading) return;

		loading = true;
		error = null;

		try {
			const invoiceData: CreateInvoiceRequest = {
				name: form.name.trim(),
				amount: form.amount,
				currency: form.currency?.trim() || 'USD',
				dueDate: form.dueDate || undefined,
				cashierId: form.cashierId || undefined
			};

			const invoice = await invoiceApi.createInvoice(invoiceData);
			
			// Redirect to the created invoice
			goto(`/invoices/${invoice.invoiceId}`);
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to create invoice';
			console.error('Error creating invoice:', err);
		} finally {
			loading = false;
		}
	}

	function goBack() {
		goto('/invoices');
	}

	function resetForm() {
		form = {
			name: '',
			amount: 0,
			currency: 'USD',
			dueDate: '',
			cashierId: undefined
		};
		formErrors = {};
		error = null;
	}

</script>

<svelte:head>
	<title>Create Invoice - Billing System</title>
</svelte:head>

<div class="container mx-auto p-6 space-y-8">
	<!-- Header with back navigation -->
	<div class="flex items-center justify-between">
		<div class="flex items-center gap-4">
			<Button variant="ghost" onclick={goBack} class="gap-2">
				<ArrowLeft size={16} />
				Back to Invoices
			</Button>
			<div class="h-6 w-px bg-border"></div>
			<div class="flex items-center gap-2">
				<FileText size={20} class="text-primary" />
				<h1 class="text-2xl font-bold">Create New Invoice</h1>
			</div>
		</div>
		<Button variant="outline" onclick={resetForm} disabled={loading}>
			Reset Form
		</Button>
	</div>

	{#if error}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="p-4">
				<div class="flex items-center gap-2 text-destructive">
					<div class="font-medium">Error creating invoice</div>
				</div>
				<p class="text-sm text-destructive/80 mt-1">{error}</p>
			</CardContent>
		</Card>
	{/if}

	<div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
		<!-- Main Form -->
		<div class="lg:col-span-2">
			<Card>
				<CardHeader>
					<CardTitle>Invoice Details</CardTitle>
					<p class="text-sm text-muted-foreground">
						Fill in the information for your new invoice
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
							<CurrencyInput
								id="amount"
								bind:value={form.amount}
								currency={form.currency || 'USD'}
								placeholder="Enter amount"
								required
								error={formErrors.amount}
							/>
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
						<Select
							id="cashier"
							bind:value={form.cashierId}
							options={cashierOptions}
							placeholder="Select cashier"
						/>
						<p class="text-xs text-muted-foreground">
							Assign a cashier to handle payments for this invoice
						</p>
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
						onclick={createInvoice} 
						disabled={loading || !form.name || form.amount <= 0}
						class="w-full gap-2"
					>
						{#if loading}
							<div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
						{:else}
							<Save size={16} />
						{/if}
						Create Invoice
					</Button>
					
					<Button variant="outline" onclick={goBack} disabled={loading} class="w-full">
						Cancel
					</Button>
				</CardContent>
			</Card>

			<!-- Help Card -->
			<Card>
				<CardContent class="p-4">
					<h4 class="font-medium mb-2">Tips</h4>
					<ul class="text-sm text-muted-foreground space-y-1">
						<li>• Use descriptive names for easy identification</li>
						<li>• Set due dates to track payment deadlines</li>
						<li>• Assign cashiers for payment processing</li>
						<li>• Double-check amounts before creating</li>
					</ul>
				</CardContent>
			</Card>
		</div>
	</div>
</div>