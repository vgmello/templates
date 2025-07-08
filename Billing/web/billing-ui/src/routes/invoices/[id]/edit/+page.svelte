<script lang="ts">
	import { goto } from '$app/navigation';
	import { enhance } from '$app/forms';
	import { Button } from '$lib/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/ui/card';
	import { Input } from '$lib/ui/input';
	import { Select } from '$lib/ui/select';
	import { ArrowLeft, Save, FileText, DollarSign, Calendar, User } from '@lucide/svelte';
	import type { PageData, ActionData } from './$types';

	type Props = {
		data: PageData;
		form: ActionData;
	};

	let { data, form: formResult }: Props = $props();

	let loading = $state(false);

	// Initialize form with server data
	let formData = $state({
		name: data.invoice.name || '',
		amount: data.invoice.amount || 0,
		currency: data.invoice.currency || 'USD',
		dueDate: data.invoice.dueDate ? data.invoice.dueDate.split('T')[0] : '',
		cashierId: data.invoice.cashierId || undefined
	});

	// Currency options
	const currencyOptions = [
		{ value: 'USD', label: 'USD - US Dollar' },
		{ value: 'EUR', label: 'EUR - Euro' },
		{ value: 'GBP', label: 'GBP - British Pound' },
		{ value: 'JPY', label: 'JPY - Japanese Yen' },
		{ value: 'CAD', label: 'CAD - Canadian Dollar' },
		{ value: 'AUD', label: 'AUD - Australian Dollar' }
	];

	// Cashier options from server data
	const cashierOptions = $derived([
		{ value: undefined, label: 'No cashier assigned' },
		...data.cashiers.map((cashier) => ({
			value: cashier.cashierId,
			label: cashier.name
		}))
	]);

	function goBack() {
		goto(`/invoices/${data.invoice.invoiceId}`);
	}

	function cancelEdit() {
		goto(`/invoices/${data.invoice.invoiceId}`);
	}
</script>

<svelte:head>
	<title>Edit {data.invoice.name || 'Invoice'} - Billing System</title>
</svelte:head>

<div class="container mx-auto space-y-8 p-6">
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

	<!-- Form Error Display -->
	{#if formResult?.errors?.form}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="p-4">
				<div class="flex items-center gap-2 text-destructive">
					<div class="font-medium">Error</div>
				</div>
				<p class="mt-1 text-sm text-destructive/80">{formResult.errors.form}</p>
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
						<CardTitle>Edit Invoice Details</CardTitle>
						<p class="text-sm text-muted-foreground">
							Update the information for this invoice
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
								value={formResult?.values?.name ?? formData.name}
								placeholder="Enter invoice description"
								class={formResult?.errors?.name ? 'border-destructive' : ''}
								disabled={loading}
								required
							/>
							{#if formResult?.errors?.name}
								<p class="text-sm text-destructive">{formResult.errors.name}</p>
							{/if}
						</div>

						<!-- Amount and Currency -->
						<div class="grid grid-cols-1 gap-4 md:grid-cols-2">
							<div class="space-y-2">
								<label
									for="amount"
									class="flex items-center gap-2 text-sm font-medium"
								>
									<DollarSign size={14} />
									Amount *
								</label>
								<Input
									id="amount"
									name="amount"
									type="number"
									step="0.01"
									min="0"
									value={formResult?.values?.amount ?? formData.amount}
									placeholder="0.00"
									class={formResult?.errors?.amount ? 'border-destructive' : ''}
									disabled={loading}
									required
								/>
								{#if formResult?.errors?.amount}
									<p class="text-sm text-destructive">{formResult.errors.amount}</p>
								{/if}
							</div>

							<div class="space-y-2">
								<label for="currency" class="text-sm font-medium">Currency *</label>
								<select 
									id="currency"
									name="currency"
									value={formResult?.values?.currency ?? formData.currency}
									class={`w-full rounded-md border border-input bg-background px-3 py-2 text-sm ${formResult?.errors?.currency ? 'border-destructive' : ''}`}
									disabled={loading}
									required
								>
									{#each currencyOptions as option}
										<option value={option.value}>{option.label}</option>
									{/each}
								</select>
								{#if formResult?.errors?.currency}
									<p class="text-sm text-destructive">{formResult.errors.currency}</p>
								{/if}
							</div>
						</div>

						<!-- Due Date -->
						<div class="space-y-2">
							<label
								for="dueDate"
								class="flex items-center gap-2 text-sm font-medium"
							>
								<Calendar size={14} />
								Due Date (Optional)
							</label>
							<Input
								id="dueDate"
								name="dueDate"
								type="date"
								value={formResult?.values?.dueDate ?? formData.dueDate}
								class={formResult?.errors?.dueDate ? 'border-destructive' : ''}
								disabled={loading}
							/>
							{#if formResult?.errors?.dueDate}
								<p class="text-sm text-destructive">{formResult.errors.dueDate}</p>
							{/if}
						</div>

						<!-- Cashier Selection -->
						<div class="space-y-2">
							<label
								for="cashier"
								class="flex items-center gap-2 text-sm font-medium"
							>
								<User size={14} />
								Assigned Cashier (Optional)
							</label>
							<select 
								id="cashier"
								name="cashierId"
								value={formResult?.values?.cashierId ?? formData.cashierId ?? ''}
								class="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
								disabled={loading}
							>
								{#each cashierOptions as option}
									<option value={option.value ?? ''}>{option.label}</option>
								{/each}
							</select>
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
								<p class="font-medium">{(formResult?.values?.name ?? formData.name) || 'Untitled Invoice'}</p>
							</div>

							<div>
								<label class="text-xs text-muted-foreground">Amount</label>
								<p class="text-lg font-bold">
									{new Intl.NumberFormat('en-US', {
										style: 'currency',
										currency: (formResult?.values?.currency ?? formData.currency) || 'USD'
									}).format(Number((formResult?.values?.amount ?? formData.amount)) || 0)}
								</p>
							</div>

							{#if (formResult?.values?.dueDate ?? formData.dueDate)}
								<div>
									<label class="text-xs text-muted-foreground">Due Date</label>
									<p>
										{new Date(formResult?.values?.dueDate ?? formData.dueDate).toLocaleDateString('en-US', {
											weekday: 'long',
											year: 'numeric',
											month: 'long',
											day: 'numeric'
										})}
									</p>
								</div>
							{/if}

							{#if (formResult?.values?.cashierId ?? formData.cashierId)}
								<div>
									<label class="text-xs text-muted-foreground"
										>Assigned Cashier</label
									>
									<p>
										{data.cashiers.find((c) => c.cashierId === (formResult?.values?.cashierId ?? formData.cashierId))?.name}
									</p>
								</div>
							{/if}
						</div>
					</CardContent>
				</Card>

				<!-- Actions -->
				<Card>
					<CardContent class="space-y-3 p-4">
						<Button
							type="submit"
							disabled={loading}
							class="w-full gap-2"
						>
							{#if loading}
								<div
									class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"
								></div>
							{:else}
								<Save size={16} />
							{/if}
							{loading ? 'Saving...' : 'Save Changes'}
						</Button>

						<Button
							type="button"
							variant="outline"
							onclick={cancelEdit}
							disabled={loading}
							class="w-full"
						>
							Cancel
						</Button>
					</CardContent>
				</Card>
		</div>
	</form>
</div>
