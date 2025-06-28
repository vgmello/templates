<script lang="ts">
	import { enhance } from '$app/forms';
	import Button from '$lib/components/ui/button.svelte';
	import Card from '$lib/components/ui/card.svelte';
	import Input from '$lib/components/ui/input.svelte';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();
	
	let isSubmitting = $state(false);
	let name = $state(form?.name || '');
	let amount = $state(form?.amount || '');
	let currency = $state(form?.currency || 'USD');
	let dueDate = $state(form?.dueDate || '');
	let cashierId = $state(form?.cashierId || '');

	// Common currencies
	const commonCurrencies = ['USD', 'EUR', 'GBP', 'CAD', 'AUD'];

	// Get tomorrow's date as default minimum
	const tomorrow = new Date();
	tomorrow.setDate(tomorrow.getDate() + 1);
	const minDate = tomorrow.toISOString().split('T')[0];

	// Real-time validation using $derived - Svelte 5 best practice
	const nameValidation = $derived.by(() => {
		const trimmedName = String(name || '').trim();
		if (!trimmedName) {
			return { isValid: false, message: 'Invoice name is required' };
		}
		if (trimmedName.length < 2) {
			return { isValid: false, message: 'Invoice name must be at least 2 characters' };
		}
		if (trimmedName.length > 100) {
			return { isValid: false, message: 'Invoice name must be less than 100 characters' };
		}
		return { isValid: true, message: '' };
	});

	const amountValidation = $derived.by(() => {
		const amountStr = String(amount || '').trim();
		const numAmount = parseFloat(amountStr);
		if (!amountStr) {
			return { isValid: false, message: 'Amount is required' };
		}
		if (isNaN(numAmount) || numAmount <= 0) {
			return { isValid: false, message: 'Amount must be greater than 0' };
		}
		if (numAmount > 1000000) {
			return { isValid: false, message: 'Amount must be less than 1,000,000' };
		}
		return { isValid: true, message: '' };
	});

	const dueDateValidation = $derived.by(() => {
		if (!dueDate) {
			return { isValid: true, message: '' }; // Optional field
		}
		
		// Convert the date string to a proper date object and compare as date strings to avoid timezone issues
		const selectedDateStr = dueDate; // Already in YYYY-MM-DD format
		const today = new Date();
		const todayStr = today.toISOString().split('T')[0]; // Get today in YYYY-MM-DD format
		
		if (selectedDateStr < todayStr) {
			return { isValid: false, message: 'Due date cannot be in the past' };
		}
		return { isValid: true, message: '' };
	});

	// Overall form validity
	const isFormValid = $derived(
		nameValidation.isValid && 
		amountValidation.isValid && 
		dueDateValidation.isValid
	);

	// Auto-focus management
	let nameInput: HTMLInputElement;
	$effect(() => {
		if (nameInput) {
			nameInput.focus();
		}
	});
</script>

<svelte:head>
	<title>Create Invoice - Billing System</title>
	<meta name="description" content="Create a new invoice" />
</svelte:head>

<div class="container mx-auto px-4 py-8 max-w-2xl">
	<!-- Header -->
	<div class="mb-8">
		<h1 class="text-3xl font-bold tracking-tight">Create Invoice</h1>
		<p class="text-muted-foreground">
			Add a new invoice to the system
		</p>
	</div>

	<!-- Error Message -->
	{#if form?.error}
		<div class="bg-destructive/15 text-destructive px-4 py-3 rounded-md mb-6">
			{form.error}
		</div>
	{/if}

	<!-- Form -->
	<Card class="p-6">
		<div class="mb-6">
			<h2 class="text-lg font-semibold">Invoice Details</h2>
			<p class="text-sm text-muted-foreground">
				Enter the details for the new invoice
			</p>
		</div>
			<form 
				method="POST" 
				use:enhance={() => {
					isSubmitting = true;
					return async ({ update }) => {
						await update();
						isSubmitting = false;
					};
				}}
				class="space-y-6"
			>
				<!-- Invoice Name -->
				<div class="space-y-2">
					<label for="name" class="text-sm font-medium">Invoice Name *</label>
					<Input
						bind:this={nameInput}
						id="name"
						name="name"
						type="text"
						placeholder="Enter invoice name (e.g., Website Development)"
						bind:value={name}
						required
						disabled={isSubmitting}
						class={!nameValidation.isValid && String(name || '').trim() ? 'border-destructive' : ''}
					/>
					{#if !nameValidation.isValid && String(name || '').trim()}
						<p class="text-sm text-destructive">{nameValidation.message}</p>
					{:else}
						<p class="text-sm text-muted-foreground">
							A descriptive name for this invoice
						</p>
					{/if}
				</div>

				<!-- Amount -->
				<div class="space-y-2">
					<label for="amount" class="text-sm font-medium">Amount *</label>
					<Input
						id="amount"
						name="amount"
						type="number"
						step="0.01"
						min="0.01"
						placeholder="0.00"
						bind:value={amount}
						required
						disabled={isSubmitting}
						class={!amountValidation.isValid && String(amount || '').trim() ? 'border-destructive' : ''}
					/>
					{#if !amountValidation.isValid && String(amount || '').trim()}
						<p class="text-sm text-destructive">{amountValidation.message}</p>
					{:else}
						<p class="text-sm text-muted-foreground">
							Invoice amount (must be greater than 0)
						</p>
					{/if}
				</div>

				<!-- Currency -->
				<div class="space-y-2">
					<label for="currency" class="text-sm font-medium">Currency</label>
					<select
						id="currency"
						name="currency"
						bind:value={currency}
						disabled={isSubmitting}
						class="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
					>
						{#each commonCurrencies as curr}
							<option value={curr}>{curr}</option>
						{/each}
					</select>
					<p class="text-sm text-muted-foreground">
						Select the currency for this invoice
					</p>
				</div>

				<!-- Due Date -->
				<div class="space-y-2">
					<label for="dueDate" class="text-sm font-medium">Due Date</label>
					<Input
						id="dueDate"
						name="dueDate"
						type="date"
						min={minDate}
						bind:value={dueDate}
						disabled={isSubmitting}
						class={!dueDateValidation.isValid && dueDate ? 'border-destructive' : ''}
					/>
					{#if !dueDateValidation.isValid && dueDate}
						<p class="text-sm text-destructive">{dueDateValidation.message}</p>
					{:else}
						<p class="text-sm text-muted-foreground">
							Optional: When payment is due
						</p>
					{/if}
				</div>

				<!-- Cashier Selection -->
				<div class="space-y-2">
					<label for="cashierId" class="text-sm font-medium">Assigned Cashier</label>
					<select
						id="cashierId"
						name="cashierId"
						bind:value={cashierId}
						disabled={isSubmitting}
						class="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
					>
						<option value="">No cashier assigned</option>
						{#each data.cashiers as cashier}
							<option value={cashier.cashierId}>{cashier.name} ({cashier.email})</option>
						{/each}
					</select>
					<p class="text-sm text-muted-foreground">
						Optional: Assign a cashier to handle this invoice
					</p>
				</div>

				<!-- Actions -->
				<div class="flex gap-4 pt-4">
					<Button 
						type="submit" 
						disabled={isSubmitting || !isFormValid} 
						class="flex-1"
						title={!isFormValid ? 'Please fix validation errors before submitting' : ''}
					>
						{isSubmitting ? 'Creating...' : 'Create Invoice'}
					</Button>
					<Button type="button" variant="outline" href="/invoices" disabled={isSubmitting}>
						Cancel
					</Button>
				</div>
				
				<!-- Form validity indicator -->
				{#if !isFormValid && (String(name || '').trim() || String(amount || '').trim() || dueDate)}
					<div class="text-sm text-muted-foreground text-center mt-2">
						Please fix the validation errors above to continue
					</div>
				{/if}
			</form>
	</Card>
</div>