<script lang="ts">
	import { onMount } from 'svelte';
	import { cashierService, type Cashier } from '$lib/api/cashiers';

	let cashiers = $state<Cashier[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		try {
			loading = true;
			const response = await cashierService.getCashiers(50, 0);
			cashiers = response.cashiers;
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load cashiers';
			console.error('Error loading cashiers:', err);
		} finally {
			loading = false;
		}
	});

	async function deleteCashier(cashierId: string) {
		if (!confirm('Are you sure you want to delete this cashier?')) {
			return;
		}

		try {
			await cashierService.deleteCashier(cashierId);
			// Remove the deleted cashier from the list
			cashiers = cashiers.filter(c => c.cashierId !== cashierId);
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to delete cashier';
			console.error('Error deleting cashier:', err);
		}
	}
</script>

<svelte:head>
	<title>Cashiers - Billing Management</title>
</svelte:head>

<div class="py-10">
	<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
		<!-- Header -->
		<div class="sm:flex sm:items-center">
			<div class="sm:flex-auto">
				<h1 class="text-base font-semibold leading-6 text-gray-900">Cashiers</h1>
				<p class="mt-2 text-sm text-gray-700">
					Manage all cashiers in the billing system.
				</p>
			</div>
			<div class="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
				<a
					href="/cashiers/new"
					class="block rounded-md bg-indigo-600 px-3 py-2 text-center text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
				>
					Add Cashier
				</a>
			</div>
		</div>

		<!-- Error message -->
		{#if error}
			<div class="mt-4 rounded-md bg-red-50 p-4">
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

		<!-- Loading state -->
		{#if loading}
			<div class="mt-8 flow-root">
				<div class="animate-pulse">
					<div class="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
						<div class="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
							<div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
								<div class="min-w-full bg-white">
									<div class="bg-gray-50 h-12"></div>
									{#each Array(5) as _}
										<div class="border-t border-gray-200">
											<div class="px-6 py-4 whitespace-nowrap">
												<div class="h-4 bg-gray-200 rounded w-1/4"></div>
											</div>
										</div>
									{/each}
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		{:else}
			<!-- Cashiers table -->
			<div class="mt-8 flow-root">
				<div class="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
					<div class="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
						<div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
							<table class="min-w-full divide-y divide-gray-300">
								<thead class="bg-gray-50">
									<tr>
										<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
											Name
										</th>
										<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
											Email
										</th>
										<th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
											ID
										</th>
										<th scope="col" class="relative px-6 py-3">
											<span class="sr-only">Actions</span>
										</th>
									</tr>
								</thead>
								<tbody class="bg-white divide-y divide-gray-200">
									{#each cashiers as cashier (cashier.cashierId)}
										<tr>
											<td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
												{cashier.name}
											</td>
											<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
												{cashier.email || 'N/A'}
											</td>
											<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 font-mono">
												{cashier.cashierId}
											</td>
											<td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
												<a 
													href="/cashiers/{cashier.cashierId}" 
													class="text-indigo-600 hover:text-indigo-900 mr-4"
												>
													Edit
												</a>
												<button 
													type="button"
													onclick={() => deleteCashier(cashier.cashierId)}
													class="text-red-600 hover:text-red-900"
												>
													Delete
												</button>
											</td>
										</tr>
									{:else}
										<tr>
											<td colspan="4" class="px-6 py-4 text-center text-sm text-gray-500">
												No cashiers found. <a href="/cashiers/new" class="text-indigo-600 hover:text-indigo-900">Create your first cashier</a>
											</td>
										</tr>
									{/each}
								</tbody>
							</table>
						</div>
					</div>
				</div>
			</div>
		{/if}
	</div>
</div>