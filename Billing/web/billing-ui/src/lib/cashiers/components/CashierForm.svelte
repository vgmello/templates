<script lang="ts">
	import { enhance } from '$app/forms';
	import { Button } from '$lib/ui/button';
	import { Input } from '$lib/ui/input';
	import { Select } from '$lib/ui/select';
	import { Badge } from '$lib/ui/badge';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/ui/card';
	import { Save, ArrowLeft, Plus, X } from '@lucide/svelte';
	import { Cashier } from '../models/Cashier';
	import { CurrencyValue, type Currency } from '$lib/core/values/Currency';
	// ActionData type is generic for form errors
	type ActionData = {
		success?: boolean;
		errors?: Record<string, string>;
		values?: Record<string, string>;
	} | null;

	type Props = {
		cashier?: Cashier;
		form?: ActionData;
		loading?: boolean;
		onCancel: () => void;
		submitLabel?: string;
		title: string;
		description: string;
	};

	let { 
		cashier = $bindable(new Cashier()), 
		form, 
		loading = false, 
		onCancel, 
		submitLabel = 'Save', 
		title, 
		description 
	}: Props = $props();

	// Reactive validation
	let validationErrors = $derived(cashier.validate());
	let nameError = $derived(validationErrors.find(e => e.includes('name')));
	let emailError = $derived(validationErrors.find(e => e.includes('email')));
	let phoneError = $derived(validationErrors.find(e => e.includes('phone')));
	let currencyError = $derived(validationErrors.find(e => e.includes('currency')));
	let isValid = $derived(validationErrors.length === 0);

	// Currency management
	let selectedCurrency = $state<Currency>('USD');
	let availableCurrencies = $derived(
		CurrencyValue.all().filter(c => !cashier.supportedCurrencies.includes(c))
	);

	function addCurrency() {
		if (selectedCurrency && !cashier.supportedCurrencies.includes(selectedCurrency)) {
			cashier.addSupportedCurrency(selectedCurrency);
			// Reset selection to first available currency
			const remaining = availableCurrencies.filter(c => c !== selectedCurrency);
			selectedCurrency = remaining[0] || 'USD';
		}
	}

	function removeCurrency(currency: Currency) {
		cashier.removeSupportedCurrency(currency);
	}

	function toggleActive() {
		if (cashier.isActive) {
			cashier.deactivate();
		} else {
			cashier.activate();
		}
	}

	// Currency options for select
	const currencyOptions = $derived(
		availableCurrencies.map(currency => ({
			value: currency,
			label: `${currency} - ${new CurrencyValue(currency).getName()}`
		}))
	);
</script>

<div class="container mx-auto max-w-2xl p-6">
	<div class="mb-6 flex items-center gap-4">
		<Button variant="outline" size="sm" onclick={onCancel}>
			<ArrowLeft size={16} />
			Back
		</Button>
		<div>
			<h1 class="text-3xl font-bold tracking-tight">{title}</h1>
			<p class="text-muted-foreground">{description}</p>
		</div>
	</div>

	<Card>
		<CardHeader>
			<CardTitle>Cashier Information</CardTitle>
		</CardHeader>
		<CardContent>
			<form
				method="POST"
				class="space-y-6"
				use:enhance={() => {
					loading = true;
					return async ({ update }) => {
						loading = false;
						await update();
					};
				}}
			>
				{#if form?.errors?.form}
					<div class="rounded-md border border-destructive/20 bg-destructive/10 p-4 text-destructive">
						{form.errors.form}
					</div>
				{/if}

				<!-- Basic Information -->
				<div class="space-y-4">
					<h3 class="text-lg font-medium">Basic Information</h3>
					
					<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
						<!-- Name -->
						<div class="space-y-2">
							<label for="name" class="text-sm font-medium">Name *</label>
							<Input
								id="name"
								name="name"
								bind:value={cashier.name}
								placeholder="Enter cashier name"
								class={nameError || form?.errors?.name ? 'border-destructive' : ''}
								disabled={loading}
								required
							/>
							{#if nameError || form?.errors?.name}
								<p class="text-sm text-destructive">{nameError || form?.errors?.name}</p>
							{/if}
						</div>

						<!-- Email -->
						<div class="space-y-2">
							<label for="email" class="text-sm font-medium">Email</label>
							<Input
								id="email"
								name="email"
								type="email"
								bind:value={cashier.email}
								placeholder="Enter email address"
								class={emailError || form?.errors?.email ? 'border-destructive' : ''}
								disabled={loading}
							/>
							{#if emailError || form?.errors?.email}
								<p class="text-sm text-destructive">{emailError || form?.errors?.email}</p>
							{/if}
						</div>
					</div>

					<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
						<!-- Phone -->
						<div class="space-y-2">
							<label for="phone" class="text-sm font-medium">Phone</label>
							<Input
								id="phone"
								name="phone"
								type="tel"
								bind:value={cashier.phone}
								placeholder="Enter phone number"
								class={phoneError ? 'border-destructive' : ''}
								disabled={loading}
							/>
							{#if phoneError}
								<p class="text-sm text-destructive">{phoneError}</p>
							{/if}
						</div>

						<!-- Status -->
						<div class="space-y-2">
							<label class="text-sm font-medium">Status</label>
							<div class="flex items-center gap-2">
								<Badge variant={cashier.isActive ? 'default' : 'secondary'}>
									{cashier.isActive ? 'Active' : 'Inactive'}
								</Badge>
								<Button
									type="button"
									variant="outline"
									size="sm"
									onclick={toggleActive}
									disabled={loading}
								>
									{cashier.isActive ? 'Deactivate' : 'Activate'}
								</Button>
							</div>
						</div>
					</div>
				</div>

				<!-- Supported Currencies -->
				<div class="space-y-4">
					<h3 class="text-lg font-medium">Supported Currencies</h3>
					
					<!-- Current currencies -->
					{#if cashier.supportedCurrencies.length > 0}
						<div class="space-y-2">
							<label class="text-sm font-medium">Current Currencies</label>
							<div class="flex flex-wrap gap-2">
								{#each cashier.supportedCurrencies as currency}
									<Badge variant="outline" class="gap-1">
										{currency} - {new CurrencyValue(currency).getName()}
										<button
											type="button"
											onclick={() => removeCurrency(currency)}
											class="ml-1 hover:text-destructive"
											disabled={loading || cashier.supportedCurrencies.length === 1}
										>
											<X size={12} />
										</button>
									</Badge>
								{/each}
							</div>
						</div>
					{/if}

					<!-- Add currency -->
					{#if availableCurrencies.length > 0}
						<div class="space-y-2">
							<label class="text-sm font-medium">Add Currency</label>
							<div class="flex gap-2">
								<Select
									bind:value={selectedCurrency}
									options={currencyOptions}
									placeholder="Select currency"
									disabled={loading}
								/>
								<Button
									type="button"
									variant="outline"
									size="sm"
									onclick={addCurrency}
									disabled={loading || !selectedCurrency}
								>
									<Plus size={16} />
									Add
								</Button>
							</div>
						</div>
					{/if}

					{#if currencyError}
						<p class="text-sm text-destructive">{currencyError}</p>
					{/if}
				</div>

				<!-- Hidden fields for form submission -->
				<input type="hidden" name="supportedCurrencies" value={JSON.stringify(cashier.supportedCurrencies)} />
				<input type="hidden" name="isActive" value={cashier.isActive} />

				<!-- Actions -->
				<div class="flex gap-2 pt-4">
					<Button type="submit" disabled={loading || !isValid}>
						<Save size={16} />
						{loading ? 'Saving...' : submitLabel}
					</Button>
					<Button type="button" variant="outline" onclick={onCancel} disabled={loading}>
						Cancel
					</Button>
				</div>
			</form>
		</CardContent>
	</Card>
</div>