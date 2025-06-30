<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { cashierService, type Cashier } from '$lib/api/cashiers';

	let cashier = $state<Cashier | null>(null);
	let name = $state('');
	let email = $state('');
	let loading = $state(true);
	let saving = $state(false);
	let error = $state<string | null>(null);
	let nameError = $state<string | null>(null);
	let emailError = $state<string | null>(null);

	const cashierId = $page.params.id;

	onMount(async () => {
		try {
			loading = true;
			cashier = await cashierService.getCashier(cashierId);
			name = cashier.name;
			email = cashier.email || '';
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load cashier';
			console.error('Error loading cashier:', err);
		} finally {
			loading = false;
		}
	});

	function validateForm() {
		let isValid = true;
		nameError = null;
		emailError = null;

		if (!name.trim()) {
			nameError = 'Name is required';
			isValid = false;
		}

		if (!email.trim()) {
			emailError = 'Email is required';
			isValid = false;
		} else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
			emailError = 'Please enter a valid email address';
			isValid = false;
		}

		return isValid;
	}

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		if (!validateForm()) {
			return;
		}

		try {
			saving = true;
			error = null;
			
			const updatedCashier = await cashierService.updateCashier(cashierId, name.trim(), email.trim());
			cashier = updatedCashier;
			
			// Redirect to cashiers list on success
			goto('/cashiers');
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to update cashier';
			console.error('Error updating cashier:', err);
		} finally {
			saving = false;
		}
	}

	async function handleDelete() {
		if (!confirm('Are you sure you want to delete this cashier? This action cannot be undone.')) {
			return;
		}

		try {
			await cashierService.deleteCashier(cashierId);
			goto('/cashiers');
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to delete cashier';
			console.error('Error deleting cashier:', err);
		}
	}

	function handleCancel() {
		goto('/cashiers');
	}
</script>

<svelte:head>
	<title>{cashier ? `Edit ${cashier.name}` : 'Edit Cashier'} - Billing Management</title>
</svelte:head>

<div class="py-10">
	<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
		<!-- Header -->
		<div class="mb-8">
			<div class="flex items-center justify-between">
				<div>
					<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
						{#if loading}
							Loading...
						{:else if cashier}
							Edit {cashier.name}
						{:else}
							Cashier Not Found
						{/if}
					</h1>
					<p class="mt-2 text-sm text-gray-700">
						Update cashier information in the billing system.
					</p>
				</div>
				{#if cashier}
					<button
						type="button"
						onclick={handleDelete}
						class="rounded-md bg-red-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-red-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-red-600"
					>
						Delete Cashier
					</button>
				{/if}
			</div>
		</div>

		<!-- Loading state -->
		{#if loading}
			<div class="max-w-2xl">
				<div class="animate-pulse space-y-6">
					<div>
						<div class="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
						<div class="h-10 bg-gray-200 rounded"></div>
					</div>
					<div>
						<div class="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
						<div class="h-10 bg-gray-200 rounded"></div>
					</div>
					<div class="flex justify-end gap-x-3">
						<div class="h-10 bg-gray-200 rounded w-20"></div>
						<div class="h-10 bg-gray-200 rounded w-32"></div>
					</div>
				</div>
			</div>
		{:else if !cashier}
			<!-- Not found state -->
			<div class="text-center">
				<svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 15.5c-.77.833.192 2.5 1.732 2.5z" />
				</svg>
				<h3 class="mt-2 text-sm font-medium text-gray-900">Cashier not found</h3>
				<p class="mt-1 text-sm text-gray-500">The cashier you're looking for doesn't exist.</p>
				<div class="mt-6">
					<a
						href="/cashiers"
						class="inline-flex items-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
					>
						Back to Cashiers
					</a>
				</div>
			</div>
		{:else}
			<!-- Form -->
			<div class="max-w-2xl">
				<!-- Error message -->
				{#if error}
					<div class="mb-6 rounded-md bg-red-50 p-4">
						<div class="flex">
							<div class="flex-shrink-0">
								<svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
									<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z" clip-rule="evenodd" />
								</svg>
							</div>
							<div class="ml-3">
								<h3 class="text-sm font-medium text-red-800">Error</h3>
								<div class="mt-2 text-sm text-red-700">
									<p>{error}</p>
								</div>
							</div>
						</div>
					</div>
				{/if}

				<!-- Cashier ID display -->
				<div class="mb-6 rounded-lg bg-gray-50 p-4">
					<div class="flex items-center">
						<div class="flex-shrink-0">
							<svg class="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
							</svg>
						</div>
						<div class="ml-2">
							<p class="text-sm text-gray-600">Cashier ID: <span class="font-mono">{cashier.cashierId}</span></p>
						</div>
					</div>
				</div>

				<form onsubmit={handleSubmit} class="space-y-6">
					<!-- Name field -->
					<div>
						<label for="name" class="block text-sm font-medium leading-6 text-gray-900">
							Name <span class="text-red-500">*</span>
						</label>
						<div class="mt-2">
							<input
								type="text"
								id="name"
								bind:value={name}
								class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
								class:ring-red-300={nameError}
								class:focus:ring-red-500={nameError}
								placeholder="Enter cashier name"
								disabled={saving}
							/>
							{#if nameError}
								<p class="mt-2 text-sm text-red-600">{nameError}</p>
							{/if}
						</div>
					</div>

					<!-- Email field -->
					<div>
						<label for="email" class="block text-sm font-medium leading-6 text-gray-900">
							Email <span class="text-red-500">*</span>
						</label>
						<div class="mt-2">
							<input
								type="email"
								id="email"
								bind:value={email}
								class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
								class:ring-red-300={emailError}
								class:focus:ring-red-500={emailError}
								placeholder="Enter email address"
								disabled={saving}
							/>
							{#if emailError}
								<p class="mt-2 text-sm text-red-600">{emailError}</p>
							{/if}
						</div>
					</div>

					<!-- Form actions -->
					<div class="flex justify-end gap-x-3">
						<button
							type="button"
							onclick={handleCancel}
							class="rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50"
							disabled={saving}
						>
							Cancel
						</button>
						<button
							type="submit"
							class="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
							disabled={saving}
						>
							{#if saving}
								<svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white inline" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
									<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
									<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
								</svg>
								Updating...
							{:else}
								Update Cashier
							{/if}
						</button>
					</div>
				</form>
			</div>
		{/if}
	</div>
</div>