<script lang="ts">
	import { goto } from '$app/navigation';
	import { tick } from 'svelte';
	import { browser } from '$app/environment';
	import Button from "$lib/components/ui/button.svelte";
	import Card from "$lib/components/ui/card.svelte";
	import Input from "$lib/components/ui/input.svelte";
	import Label from "$lib/components/ui/label.svelte";
	import Badge from "$lib/components/ui/badge.svelte";
	import { cashierService } from "$lib/api.js";
	import { ArrowLeft, Plus, X } from "lucide-svelte";

	// State
	let name = $state('');
	let email = $state('');
	let currencies = $state(['USD']);
	let newCurrency = $state('');
	let loading = $state(false);
	let error = $state(null);

	// Element references for focus management
	let nameInput: HTMLInputElement | undefined = $state();
	let errorDiv: HTMLDivElement | undefined = $state();
	let formRef: HTMLFormElement | undefined = $state();

	const availableCurrencies = ['USD', 'EUR', 'GBP', 'CAD', 'AUD', 'JPY', 'CNY', 'INR', 'BRL'];

	// Effects for focus management and side effects
	$effect(() => {
		// Auto-focus name input when component mounts
		if (nameInput && !loading) {
			tick().then(() => nameInput?.focus());
		}
	});

	$effect(() => {
		// Scroll to and announce errors for accessibility
		if (error && errorDiv) {
			tick().then(() => {
				errorDiv?.scrollIntoView({ behavior: 'smooth', block: 'center' });
				// Announce error to screen readers
				const announcement = document.createElement('div');
				announcement.setAttribute('aria-live', 'polite');
				announcement.setAttribute('aria-atomic', 'true');
				announcement.className = 'sr-only';
				announcement.textContent = `Error: ${error}`;
				document.body.appendChild(announcement);
				setTimeout(() => document.body.removeChild(announcement), 1000);
			});
		}
	});

	$effect(() => {
		// Save form data to sessionStorage for recovery
		if (name || email || currencies.length > 1) {
			const formData = { name, email, currencies };
			sessionStorage.setItem('createCashierForm', JSON.stringify(formData));
		}
	});

	$effect(() => {
		// Restore form data on page load
		const savedData = sessionStorage.getItem('createCashierForm');
		if (savedData) {
			try {
				const { name: savedName, email: savedEmail, currencies: savedCurrencies } = JSON.parse(savedData);
				if (savedName) name = savedName;
				if (savedEmail) email = savedEmail;
				if (savedCurrencies && savedCurrencies.length > 0) currencies = savedCurrencies;
			} catch (e) {
				console.warn('Failed to restore form data:', e);
			}
		}

		// Clear saved data when form is successfully submitted
		return () => {
			if (!error && !loading) {
				sessionStorage.removeItem('createCashierForm');
			}
		};
	});

	function addCurrency() {
		const currency = newCurrency.trim().toUpperCase();
		if (currency && !currencies.includes(currency)) {
			currencies = [...currencies, currency];
			newCurrency = '';
		}
	}

	function removeCurrency(currencyToRemove) {
		currencies = currencies.filter(c => c !== currencyToRemove);
	}

	function addPredefinedCurrency(currency) {
		if (!currencies.includes(currency)) {
			currencies = [...currencies, currency];
		}
	}

	async function handleSubmit(event) {
		event.preventDefault();
		
		if (!name.trim() || !email.trim() || currencies.length === 0) {
			error = 'Please fill in all required fields and add at least one currency.';
			return;
		}

		try {
			loading = true;
			error = null;

			const cashierData = {
				name: name.trim(),
				email: email.trim(),
				currencies: currencies
			};

			// In browser context, use global fetch; on server, this won't run
			await cashierService.createCashier(cashierData, browser ? fetch : undefined);
			
			// Redirect to cashiers list
			goto('/cashiers');
		} catch (err) {
			error = err.message;
			console.error('Failed to create cashier:', err);
		} finally {
			loading = false;
		}
	}

	function handleGoBack() {
		goto('/cashiers');
	}
</script>

<svelte:head>
	<title>Create Cashier - Billing Service</title>
</svelte:head>

<div class="container mx-auto px-4 py-8">
	<div class="max-w-2xl mx-auto space-y-6">
		<!-- Header -->
		<div class="flex items-center gap-4">
			<Button variant="ghost" size="icon" onclick={handleGoBack}>
				<ArrowLeft class="h-4 w-4" />
			</Button>
			<div class="space-y-1">
				<h1 class="text-3xl font-bold tracking-tight">Create New Cashier</h1>
				<p class="text-muted-foreground">
					Add a new cashier to handle payments for your business.
				</p>
			</div>
		</div>

		<!-- Form -->
		<Card class="p-6">
			<form bind:this={formRef} onsubmit={handleSubmit} class="space-y-6">
				<!-- Basic Information -->
				<div class="space-y-4">
					<h3 class="text-lg font-semibold">Basic Information</h3>
					
					<div class="space-y-2">
						<Label for="name">Name *</Label>
						<Input
							id="name"
							bind:this={nameInput}
							bind:value={name}
							placeholder="Enter cashier name"
							required
							disabled={loading}
						/>
					</div>

					<div class="space-y-2">
						<Label for="email">Email *</Label>
						<Input
							id="email"
							type="email"
							bind:value={email}
							placeholder="Enter email address"
							required
							disabled={loading}
						/>
					</div>
				</div>

				<!-- Currencies -->
				<div class="space-y-4">
					<h3 class="text-lg font-semibold">Supported Currencies</h3>
					
					{#if currencies.length > 0}
						<div class="flex flex-wrap gap-2">
							{#each currencies as currency}
								<Badge variant="default" class="gap-1">
									{currency}
									<button
										type="button"
										onclick={() => removeCurrency(currency)}
										class="ml-1 hover:bg-primary-foreground/20 rounded-full p-0.5"
										disabled={loading}
									>
										<X class="h-3 w-3" />
									</button>
								</Badge>
							{/each}
						</div>
					{/if}

					<div class="flex gap-2">
						<Input
							bind:value={newCurrency}
							placeholder="Add currency (e.g., USD)"
							class="flex-1"
							disabled={loading}
							onkeydown={(e) => e.key === 'Enter' && (e.preventDefault(), addCurrency())}
						/>
						<Button type="button" variant="outline" onclick={addCurrency} disabled={loading || !newCurrency.trim()}>
							<Plus class="h-4 w-4" />
						</Button>
					</div>

					<div class="space-y-2">
						<p class="text-sm text-muted-foreground">Quick add popular currencies:</p>
						<div class="flex flex-wrap gap-2">
							{#each availableCurrencies as currency}
								{#if !currencies.includes(currency)}
									<Button
										type="button"
										variant="outline"
										size="sm"
										onclick={() => addPredefinedCurrency(currency)}
										disabled={loading}
									>
										{currency}
									</Button>
								{/if}
							{/each}
						</div>
					</div>
				</div>

				<!-- Error Message -->
				{#if error}
					<div bind:this={errorDiv} class="p-3 text-sm text-destructive bg-destructive/10 border border-destructive/20 rounded-md" role="alert" aria-live="polite">
						{error}
					</div>
				{/if}

				<!-- Actions -->
				<div class="flex gap-3 pt-4">
					<Button type="submit" disabled={loading} class="flex-1">
						{#if loading}
							<div class="animate-spin rounded-full h-4 w-4 border-b-2 border-current mr-2"></div>
							Creating...
						{:else}
							Create Cashier
						{/if}
					</Button>
					<Button type="button" variant="outline" onclick={handleGoBack} disabled={loading}>
						Cancel
					</Button>
				</div>
			</form>
		</Card>
	</div>
</div>