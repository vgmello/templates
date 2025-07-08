<script lang="ts">
	import { goto } from '$app/navigation';
	import { enhance } from '$app/forms';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { ArrowLeft, Save } from '@lucide/svelte';
	import type { PageData, ActionData } from './$types';

	type Props = {
		data: PageData;
		form: ActionData;
	};

	let { data, form }: Props = $props();
	let loading = $state(false);

	function handleCancel() {
		goto('/cashiers');
	}
</script>

<svelte:head>
	<title>Edit Cashier - Billing System</title>
</svelte:head>

<div class="container mx-auto max-w-2xl p-6">
	<div class="mb-6 flex items-center gap-4">
		<Button variant="outline" size="sm" onclick={handleCancel}>
			<ArrowLeft size={16} />
			Back
		</Button>
		<div>
			<h1 class="text-3xl font-bold tracking-tight">Edit Cashier</h1>
			<p class="text-muted-foreground">Update cashier information</p>
		</div>
	</div>

	<Card>
		<CardHeader>
			<CardTitle>Cashier Information</CardTitle>
		</CardHeader>
		<CardContent>
			<form
				method="POST"
				class="space-y-4"
				use:enhance={() => {
					loading = true;

					return async ({ update }) => {
						loading = false;
						await update();
					};
				}}
			>
				{#if form?.errors?.form}
					<div
						class="rounded-md border border-destructive/20 bg-destructive/10 p-4 text-destructive"
					>
						{form.errors.form}
					</div>
				{/if}

				<div class="space-y-2">
					<label for="name" class="text-sm font-medium">
						Name <span class="text-destructive">*</span>
					</label>
					<Input
						id="name"
						name="name"
						value={form?.values?.name ?? data.cashier.name}
						placeholder="Enter cashier name"
						class={form?.errors?.name ? 'border-destructive' : ''}
						disabled={loading}
						required
					/>
					{#if form?.errors?.name}
						<p class="text-sm text-destructive">{form.errors.name}</p>
					{/if}
					<p class="text-xs text-muted-foreground">
						Name must be between 2 and 100 characters
					</p>
				</div>

				<div class="space-y-2">
					<label for="email" class="text-sm font-medium">Email</label>
					<Input
						id="email"
						name="email"
						type="email"
						value={form?.values?.email ?? data.cashier.email ?? ''}
						placeholder="Enter email address (optional)"
						class={form?.errors?.email ? 'border-destructive' : ''}
						disabled={loading}
					/>
					{#if form?.errors?.email}
						<p class="text-sm text-destructive">{form.errors.email}</p>
					{/if}
					<p class="text-xs text-muted-foreground">
						Email is optional but must be valid if provided
					</p>
				</div>

				<div class="flex gap-2 pt-4">
					<Button type="submit" disabled={loading} class="flex items-center gap-2">
						{#if loading}
							<div
								class="h-4 w-4 animate-spin rounded-full border-b-2 border-white"
							></div>
						{:else}
							<Save size={16} />
						{/if}
						{loading ? 'Updating...' : 'Update Cashier'}
					</Button>
					<Button
						type="button"
						variant="outline"
						onclick={handleCancel}
						disabled={loading}
					>
						Cancel
					</Button>
				</div>
			</form>
		</CardContent>
	</Card>
</div>
