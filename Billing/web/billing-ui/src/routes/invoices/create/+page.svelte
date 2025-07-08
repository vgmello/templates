<script lang="ts">
	import { goto } from '$app/navigation';
	import { enhance } from '$app/forms';
	import { Button } from '$lib/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/ui/card';
	import { Input } from '$lib/ui/input';
	import { CurrencyInput } from '$lib/ui/currency-input';
	import { Select } from '$lib/ui/select';
	import { ArrowLeft, Save, FileText, DollarSign, Calendar, User } from '@lucide/svelte';
	import type { GetCashiersResult } from '$lib/cashiers';
	import { CreateInvoiceForm } from '$lib/invoices';
	import { formatDateForInput } from '$lib/infrastructure/utils/Date.js';
	import type { ActionData } from './$types';

	type Props = {
		data: {
			cashiers: GetCashiersResult[];
		};
		form: ActionData;
	};

	let { data, form }: Props = $props();
	let { cashiers } = data;
	let loading = $state(false);

	let formState = $state(new CreateInvoiceForm());
	
	// Initialize form with any returned values on error
	if (form?.values) {
		formState.name = form.values.name ?? '';
		formState.amount = form.values.amount?.toString() ?? '';
		formState.currency = form.values.currency ?? 'USD';
		formState.cashierId = form.values.cashierId ?? '';
		formState.dueDate = form.values.dueDate ?? '';
	}

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
		{ value: '', label: 'No cashier assigned' },
		...cashiers.map((cashier) => ({
			value: cashier.cashierId,
			label: cashier.name
		}))
	]);

	function goBack() {
		goto('/invoices');
	}
</script>

<svelte:head>
	<title>Create Invoice - Billing System</title>
</svelte:head>

<div class="container mx-auto space-y-8 p-6">
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
	</div>

	{#if form?.errors?.form}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="p-4">
				<div class="flex items-center gap-2 text-destructive">
					<div class="font-medium">Error creating invoice</div>
				</div>
				<p class="mt-1 text-sm text-destructive/80">{form.errors.form}</p>
			</CardContent>
		</Card>
	{/if}

	<form
		method="POST"
		class="grid grid-cols-1 gap-8 lg:grid-cols-3"
		use:enhance={() => {
			loading = true;

			return async ({ update }) => {
				loading = false;
				await update();
			};
		}}
	>
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
						<label for="name" class="flex items-center gap-2 text-sm font-medium">
							<FileText size={14} />
							Invoice Name *
						</label>
						<Input
							id="name"
							name="name"
							bind:value={formState.name}
							placeholder="Enter invoice description"
							class={formState.nameError || form?.errors?.name ? 'border-destructive' : ''}
							disabled={loading}
							required
						/>
						{#if formState.nameError && formState.name.length > 0}
							<p class="text-sm text-destructive">{formState.nameError}</p>
						{:else if form?.errors?.name}
							<p class="text-sm text-destructive">{form.errors.name}</p>
						{/if}
					</div>

					<!-- Amount and Currency -->
					<div class="grid grid-cols-1 gap-4 md:grid-cols-2">
						<div class="space-y-2">
							<label for="amount" class="flex items-center gap-2 text-sm font-medium">
								<DollarSign size={14} />
								Amount *
							</label>
							<Input
								id="amount"
								name="amount"
								bind:value={formState.amount}
								placeholder="Enter amount (e.g., 100.50)"
								class={formState.amountError || form?.errors?.amount ? 'border-destructive' : ''}
								disabled={loading}
								required
							/>
							{#if formState.amountError && formState.amount.length > 0}
								<p class="text-sm text-destructive">{formState.amountError}</p>
							{:else if form?.errors?.amount}
								<p class="text-sm text-destructive">{form.errors.amount}</p>
							{/if}
						</div>

						<div class="space-y-2">
							<label for="currency" class="text-sm font-medium">Currency *</label>
							<Select
								id="currency"
								bind:value={formState.currency}
								options={currencyOptions}
								placeholder="Select currency"
								error={form?.errors?.currency}
								disabled={loading}
							/>
							<input type="hidden" name="currency" value={formState.currency} />
						</div>
					</div>

					<!-- Due Date -->
					<div class="space-y-2">
						<label for="dueDate" class="flex items-center gap-2 text-sm font-medium">
							<Calendar size={14} />
							Due Date *
						</label>
						<Input
							id="dueDate"
							name="dueDate"
							type="date"
							bind:value={formState.dueDate}
							min={formatDateForInput()}
							class={formState.dueDateError || form?.errors?.dueDate ? 'border-destructive' : ''}
							disabled={loading}
							required
						/>
						{#if formState.dueDateError && formState.dueDate.length > 0}
							<p class="text-sm text-destructive">{formState.dueDateError}</p>
						{:else if form?.errors?.dueDate}
							<p class="text-sm text-destructive">{form.errors.dueDate}</p>
						{/if}
					</div>

					<!-- Cashier Selection -->
					<div class="space-y-2">
						<label for="cashier" class="flex items-center gap-2 text-sm font-medium">
							<User size={14} />
							Assigned Cashier *
						</label>
						<Select
							id="cashier"
							bind:value={formState.cashierId}
							options={cashierOptions}
							placeholder="Select cashier"
							disabled={loading}
							class={formState.cashierError ? 'border-destructive' : ''}
						/>
						<input type="hidden" name="cashierId" value={formState.cashierId} />
						{#if formState.cashierError}
							<p class="text-sm text-destructive">{formState.cashierError}</p>
						{/if}
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
							<span class="text-xs text-muted-foreground">Name</span>
							<p class="font-medium">{formState.name || 'Untitled Invoice'}</p>
						</div>

						<div>
							<span class="text-xs text-muted-foreground">Amount</span>
							<p class="text-lg font-bold">
								{new Intl.NumberFormat('en-US', {
									style: 'currency',
									currency: formState.currency || 'USD'
								}).format(parseFloat(formState.amount) || 0)}
							</p>
						</div>

						{#if formState.dueDate}
							<div>
								<span class="text-xs text-muted-foreground">Due Date</span>
								<p>
									{new Date(formState.dueDate).toLocaleDateString('en-US', {
										weekday: 'long',
										year: 'numeric',
										month: 'long',
										day: 'numeric'
									})}
								</p>
							</div>
						{/if}

						{#if formState.cashierId}
							<div>
								<span class="text-xs text-muted-foreground">Assigned Cashier</span>
								<p>{cashiers.find((c) => c.cashierId === formState.cashierId)?.name}</p>
							</div>
						{/if}
					</div>
				</CardContent>
			</Card>

			<!-- Actions -->
			<Card>
				<CardContent class="space-y-3 p-4">
					<Button type="submit" disabled={loading || !formState.isValid} class="w-full gap-2">
						{#if loading}
							<div
								class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"
							></div>
						{:else}
							<Save size={16} />
						{/if}
						{loading ? 'Creating...' : 'Create Invoice'}
					</Button>

					<Button
						type="button"
						variant="outline"
						onclick={goBack}
						disabled={loading}
						class="w-full"
					>
						Cancel
					</Button>
				</CardContent>
			</Card>

			<!-- Help Card -->
			<Card>
				<CardContent class="p-4">
					<h4 class="mb-2 font-medium">Tips</h4>
					<ul class="space-y-1 text-sm text-muted-foreground">
						<li>• Use descriptive names for easy identification</li>
						<li>• Set due dates to track payment deadlines</li>
						<li>• Assign cashiers for payment processing</li>
						<li>• Double-check amounts before creating</li>
					</ul>
				</CardContent>
			</Card>
		</div>
	</form>
</div>
