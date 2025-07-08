<script lang="ts">
	import { Badge } from '$lib/ui/badge';
	import { Button } from '$lib/ui/button';
	import { Card, CardContent } from '$lib/ui/card';
	import { Pencil, Trash2, User, Mail, Phone, Coins, Hash } from '@lucide/svelte';
	import { CurrencyValue } from '$lib/core/values/Currency';
	import type { Cashier } from '../models/Cashier';

	type Props = {
		cashier: Cashier;
		onEdit: (id: string) => void;
		onDelete: (id: string) => void;
		deleting?: boolean;
	};

	let { cashier, onEdit, onDelete, deleting = false }: Props = $props();

	// Reactive derived properties from domain model
	let statusVariant = $derived.by(() => {
		if (!cashier.isActive) return 'secondary' as const;
		if (cashier.supportedCurrencies.length === 0) return 'destructive' as const;
		return 'default' as const;
	});

	let statusText = $derived.by(() => {
		if (!cashier.isActive) return 'Inactive';
		if (cashier.supportedCurrencies.length === 0) return 'Setup Required';
		return 'Active';
	});

	let formattedCurrencies = $derived.by(() => 
		cashier.supportedCurrencies
			.map(currency => new CurrencyValue(currency).getSymbol())
			.join(', ')
	);
</script>

<Card class="relative overflow-hidden">
	<CardContent class="p-6">
		<div class="flex items-start justify-between">
			<div class="flex-1 space-y-3">
				<!-- Header -->
				<div class="flex items-center gap-3">
					<div class="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
						<User class="h-5 w-5 text-primary" />
					</div>
					<div class="flex-1">
						<h3 class="font-semibold text-lg">{cashier.displayName}</h3>
						<div class="flex items-center gap-2">
							<Badge variant={statusVariant}>{statusText}</Badge>
							{#if cashier.id}
								<span class="text-xs text-muted-foreground flex items-center gap-1">
									<Hash class="h-3 w-3" />
									{cashier.id}
								</span>
							{/if}
						</div>
					</div>
				</div>

				<!-- Contact Information -->
				<div class="space-y-2">
					{#if cashier.email}
						<div class="flex items-center gap-2 text-sm text-muted-foreground">
							<Mail class="h-4 w-4" />
							<span class="truncate">{cashier.email}</span>
						</div>
					{/if}
					
					{#if cashier.phone}
						<div class="flex items-center gap-2 text-sm text-muted-foreground">
							<Phone class="h-4 w-4" />
							<span>{cashier.phone}</span>
						</div>
					{/if}

					{#if cashier.supportedCurrencies.length > 0}
						<div class="flex items-center gap-2 text-sm text-muted-foreground">
							<Coins class="h-4 w-4" />
							<span>Currencies: {formattedCurrencies}</span>
						</div>
					{/if}
				</div>

				<!-- Currency Badges -->
				{#if cashier.supportedCurrencies.length > 0}
					<div class="flex flex-wrap gap-1">
						{#each cashier.supportedCurrencies as currency}
							<Badge variant="outline" class="text-xs">
								{currency}
							</Badge>
						{/each}
					</div>
				{/if}

				<!-- Timestamps -->
				<div class="text-xs text-muted-foreground">
					Created: {new Date(cashier.createdAt).toLocaleDateString()}
					{#if cashier.updatedAt !== cashier.createdAt}
						• Updated: {new Date(cashier.updatedAt).toLocaleDateString()}
					{/if}
				</div>
			</div>

			<!-- Actions -->
			<div class="flex flex-col gap-2 ml-4">
				<Button
					variant="outline"
					size="sm"
					onclick={() => onEdit(cashier.id)}
					disabled={deleting}
				>
					<Pencil class="h-4 w-4" />
				</Button>
				<Button
					variant="outline"
					size="sm"
					onclick={() => onDelete(cashier.id)}
					disabled={deleting}
					class="hover:border-destructive hover:text-destructive"
				>
					<Trash2 class="h-4 w-4" />
				</Button>
			</div>
		</div>

		{#if deleting}
			<div class="absolute inset-0 bg-background/80 flex items-center justify-center">
				<div class="text-sm text-muted-foreground">Deleting...</div>
			</div>
		{/if}
	</CardContent>
</Card>