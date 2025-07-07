<script lang="ts">
	import { goto } from '$app/navigation';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { ArrowLeft, Save } from '@lucide/svelte';
	import { cashierApi, type CreateCashierRequest } from '$lib';

	let form = $state<CreateCashierRequest>({
		name: '',
		email: ''
	});

	let loading = $state(false);
	let error = $state<string | null>(null);
	let fieldErrors = $state<Record<string, string>>({});

	function validateForm() {
		const errors: Record<string, string> = {};

		if (!form.name.trim()) {
			errors.name = 'Name is required';
		} else if (form.name.trim().length < 2) {
			errors.name = 'Name must be at least 2 characters';
		} else if (form.name.trim().length > 100) {
			errors.name = 'Name must not exceed 100 characters';
		}

		if (form.email && !isValidEmail(form.email)) {
			errors.email = 'Please enter a valid email address';
		}

		fieldErrors = errors;
		return Object.keys(errors).length === 0;
	}

	function isValidEmail(email: string): boolean {
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		return emailRegex.test(email);
	}

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		
		if (!validateForm()) {
			return;
		}

		loading = true;
		error = null;

		try {
			await cashierApi.createCashier({
				name: form.name.trim(),
				email: form.email.trim() || ''
			});
			
			// Navigate back to cashiers list
			goto('/cashiers');
		} catch (err: any) {
			if (err.status === 400 && err.data?.errors) {
				// Handle validation errors from API
				const apiErrors: Record<string, string> = {};
				err.data.errors.forEach((errorMsg: string) => {
					if (errorMsg.toLowerCase().includes('name')) {
						apiErrors.name = errorMsg;
					} else if (errorMsg.toLowerCase().includes('email')) {
						apiErrors.email = errorMsg;
					}
				});
				fieldErrors = { ...fieldErrors, ...apiErrors };
			} else {
				error = err instanceof Error ? err.message : 'Failed to create cashier';
			}
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
	<title>Create Cashier - Billing System</title>
</svelte:head>

<div class="container mx-auto p-6 max-w-2xl">
	<div class="flex items-center gap-4 mb-6">
		<Button variant="outline" size="sm" onclick={handleCancel}>
			<ArrowLeft size={16} />
			Back
		</Button>
		<div>
			<h1 class="text-3xl font-bold tracking-tight">Create Cashier</h1>
			<p class="text-muted-foreground">Add a new cashier to the system</p>
		</div>
	</div>

	<Card>
		<CardHeader>
			<CardTitle>Cashier Information</CardTitle>
		</CardHeader>
		<CardContent>
			<form onsubmit={handleSubmit} class="space-y-4">
				{#if error}
					<div class="p-4 border border-destructive/20 bg-destructive/10 text-destructive rounded-md">
						{error}
					</div>
				{/if}

				<div class="space-y-2">
					<label for="name" class="text-sm font-medium">
						Name <span class="text-destructive">*</span>
					</label>
					<Input
						id="name"
						bind:value={form.name}
						placeholder="Enter cashier name"
						class={fieldErrors.name ? 'border-destructive' : ''}
						disabled={loading}
					/>
					{#if fieldErrors.name}
						<p class="text-sm text-destructive">{fieldErrors.name}</p>
					{/if}
					<p class="text-xs text-muted-foreground">
						Name must be between 2 and 100 characters
					</p>
				</div>

				<div class="space-y-2">
					<label for="email" class="text-sm font-medium">Email</label>
					<Input
						id="email"
						type="email"
						bind:value={form.email}
						placeholder="Enter email address (optional)"
						class={fieldErrors.email ? 'border-destructive' : ''}
						disabled={loading}
					/>
					{#if fieldErrors.email}
						<p class="text-sm text-destructive">{fieldErrors.email}</p>
					{/if}
					<p class="text-xs text-muted-foreground">
						Email is optional but must be valid if provided
					</p>
				</div>

				<div class="flex gap-2 pt-4">
					<Button type="submit" disabled={loading} class="flex items-center gap-2">
						{#if loading}
							<div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
						{:else}
							<Save size={16} />
						{/if}
						{loading ? 'Creating...' : 'Create Cashier'}
					</Button>
					<Button type="button" variant="outline" onclick={handleCancel} disabled={loading}>
						Cancel
					</Button>
				</div>
			</form>
		</CardContent>
	</Card>
</div>