<script lang="ts">
	import { goto } from '$app/navigation';
	import { enhance } from '$app/forms';
	import { UpdateCashierForm } from '$lib/cashiers';
	import { Button } from '$lib/ui/button';
	import { Input } from '$lib/ui/input';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/ui/card';
	import { Save, ArrowLeft } from '@lucide/svelte';
	import type { PageData, ActionData } from './$types';

	type Props = {
		data: PageData;
		form: ActionData;
	};

	let { data, form }: Props = $props();
	let loading = $state(false);

	let formState = new UpdateCashierForm();

	// Load cashier data into form
	$effect(() => {
		if (data.cashier) {
			formState.loadFrom(data.cashier);
		}
	});

	function handleCancel() {
		goto('/cashiers');
	}

	let displayName = $derived(data.cashier?.name || data.cashier?.email || 'Unknown Cashier');
</script>

<svelte:head>
	<title>Edit {displayName} - Billing System</title>
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
					<div
						class="rounded-md border border-destructive/20 bg-destructive/10 p-4 text-destructive"
					>
						{form.errors.form}
					</div>
				{/if}

				<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
					<!-- Name -->
					<div class="space-y-2">
						<label for="name" class="text-sm font-medium">Name *</label>
						<Input
							id="name"
							name="name"
							bind:value={formState.name}
							placeholder="Enter cashier name"
							class={formState.nameError || form?.errors?.name
								? 'border-destructive'
								: ''}
							disabled={loading}
							required
						/>
						{#if formState.nameError && formState.name.length > 0}
							<p class="text-sm text-destructive">{formState.nameError}</p>
						{:else if form?.errors?.name}
							<p class="text-sm text-destructive">{form?.errors?.name}</p>
						{/if}
					</div>

					<!-- Email -->
					<div class="space-y-2">
						<label for="email" class="text-sm font-medium">Email *</label>
						<Input
							id="email"
							name="email"
							type="email"
							bind:value={formState.email}
							placeholder="Enter email address"
							class={formState.emailError || form?.errors?.email
								? 'border-destructive'
								: ''}
							disabled={loading}
							required
						/>
						{#if formState.emailError && formState.email.length > 0}
							<p class="text-sm text-destructive">{formState.emailError}</p>
						{:else if form?.errors?.email}
							<p class="text-sm text-destructive">{form?.errors?.email}</p>
						{/if}
					</div>
				</div>

				<!-- Actions -->
				<div class="flex gap-2 pt-4">
					<Button type="submit" disabled={loading || !formState.isValid}>
						<Save size={16} />
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
