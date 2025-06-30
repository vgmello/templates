<script lang="ts">
	import { goto } from '$app/navigation';
	import { cashierService } from '$lib/api/cashiers';

	let name = $state('');
	let email = $state('');
	let loading = $state(false);
	let error = $state<string | null>(null);
	let nameError = $state<string | null>(null);
	let emailError = $state<string | null>(null);

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
			loading = true;
			error = null;
			
			await cashierService.createCashier(name.trim(), email.trim());
			
			// Redirect to cashiers list on success
			goto('/cashiers');
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to create cashier';
			console.error('Error creating cashier:', err);
		} finally {
			loading = false;
		}
	}

	function handleCancel() {
		goto('/cashiers');
	}
</script>

<svelte:head>
	<title>New Cashier - Billing Management</title>
</svelte:head>

<div class="py-10">
	<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
		<!-- Header -->
		<div class="mb-8">
			<h1 class="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
				Create New Cashier
			</h1>
			<p class="mt-2 text-sm text-gray-700">
				Add a new cashier to the billing system.
			</p>
		</div>

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
							disabled={loading}
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
							disabled={loading}
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
						disabled={loading}
					>
						Cancel
					</button>
					<button
						type="submit"
						class="rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
						disabled={loading}
					>
						{#if loading}
							<svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white inline" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
							</svg>
							Creating...
						{:else}
							Create Cashier
						{/if}
					</button>
				</div>
			</form>
		</div>
	</div>
</div>