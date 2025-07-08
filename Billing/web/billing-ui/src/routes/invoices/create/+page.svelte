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
	import { formatDateForInput } from '$lib/infrastructure/utils/date.js';
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

	// Form state variables
	let formName = $state(form?.values?.name ?? '');
	let formAmount = $state(form?.values?.amount ?? 0);
	let formCurrency = $state(form?.values?.currency ?? 'USD');
	let formCashierId = $state(form?.values?.cashierId ?? '');

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
							bind:value={formName}
							placeholder="Enter invoice description"
							class={form?.errors?.name ? 'border-destructive' : ''}
							disabled={loading}
							required
						/>
						{#if form?.errors?.name}
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
							<CurrencyInput
								id="amount"
								bind:value={formAmount}
								currency={formCurrency || 'USD'}
								placeholder="Enter amount"
								required
								error={form?.errors?.amount}
								disabled={loading}
							/>
							<input type="hidden" name="amount" value={formAmount} />
						</div>

						<div class="space-y-2">
							<label for="currency" class="text-sm font-medium">Currency *</label>
							<Select
								id="currency"
								bind:value={formCurrency}
								options={currencyOptions}
								placeholder="Select currency"
								error={form?.errors?.currency}
								disabled={loading}
							/>
							<input type="hidden" name="currency" value={formCurrency} />
						</div>
					</div>

					<!-- Due Date -->
					<div class="space-y-2">
						<label for="dueDate" class="flex items-center gap-2 text-sm font-medium">
							<Calendar size={14} />
							Due Date (Optional)
						</label>
						<Input
							id="dueDate"
							name="dueDate"
							type="date"
							value={form?.values?.dueDate ?? ''}
							min={formatDateForInput()}
							class={form?.errors?.dueDate ? 'border-destructive' : ''}
							disabled={loading}
						/>
						{#if form?.errors?.dueDate}
							<p class="text-sm text-destructive">{form.errors.dueDate}</p>
						{/if}
					</div>

					<!-- Cashier Selection -->
					<div class="space-y-2">
						<label for="cashier" class="flex items-center gap-2 text-sm font-medium">
							<User size={14} />
							Assigned Cashier (Optional)
						</label>
						<Select
							id="cashier"
							bind:value={formCashierId}
							options={cashierOptions}
							placeholder="Select cashier"
							disabled={loading}
						/>
						<input type="hidden" name="cashierId" value={formCashierId} />
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
							<p class="font-medium">{formName || 'Untitled Invoice'}</p>
						</div>

						<div>
							<span class="text-xs text-muted-foreground">Amount</span>
							<p class="text-lg font-bold">
								{new Intl.NumberFormat('en-US', {
									style: 'currency',
									currency: formCurrency || 'USD'
								}).format(formAmount || 0)}
							</p>
						</div>

						{#if form?.values?.dueDate}
							<div>
								<span class="text-xs text-muted-foreground">Due Date</span>
								<p>
									{new Date(form.values.dueDate).toLocaleDateString('en-US', {
										weekday: 'long',
										year: 'numeric',
										month: 'long',
										day: 'numeric'
									})}
								</p>
							</div>
						{/if}

						{#if formCashierId}
							<div>
								<span class="text-xs text-muted-foreground">Assigned Cashier</span>
								<p>{cashiers.find((c) => c.cashierId === formCashierId)?.name}</p>
							</div>
						{/if}
					</div>
				</CardContent>
			</Card>

			<!-- Actions -->
			<Card>
				<CardContent class="space-y-3 p-4">
					<Button type="submit" disabled={loading} class="w-full gap-2">
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
