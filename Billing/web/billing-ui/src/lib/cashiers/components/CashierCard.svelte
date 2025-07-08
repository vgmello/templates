<script lang="ts">
	import { Badge } from '$lib/ui/badge';
	import { Button } from '$lib/ui/button';
	import { Card, CardContent } from '$lib/ui/card';
	import { Pencil, Trash2, User, Mail, Hash } from '@lucide/svelte';
	import type { GetCashiersResult } from '../CashiersApi';

	type Props = {
		cashier: GetCashiersResult;
		onEdit: (id: string) => void;
		onDelete: (id: string) => void;
		deleting?: boolean;
	};

	let { cashier, onEdit, onDelete, deleting = false }: Props = $props();
	
	// Simple derived display name
	let displayName = $derived(cashier.name || cashier.email || 'Unknown Cashier');
</script>

<Card class="relative">
	<CardContent class="p-4">
		<div class="flex gap-4">
			<div class="flex-1 space-y-3">
				<!-- Header -->
				<div class="flex items-start gap-3">
					<div class="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
						<User class="h-5 w-5 text-primary" />
					</div>
					<div class="flex-1">
						<h3 class="font-semibold text-lg">{displayName}</h3>
						<div class="flex items-center gap-2">
							<Badge variant="default">Active</Badge>
							<span class="text-xs text-muted-foreground flex items-center gap-1">
								<Hash class="h-3 w-3" />
								{cashier.cashierId}
							</span>
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
				</div>

				<!-- Timestamps -->
				{#if cashier.createdDateUtc}
					<div class="text-xs text-muted-foreground">
						Created: {new Date(cashier.createdDateUtc).toLocaleDateString()}
					</div>
				{/if}
			</div>

			<!-- Actions -->
			<div class="flex flex-col gap-2 ml-4">
				<Button
					variant="outline"
					size="sm"
					onclick={() => onEdit(cashier.cashierId)}
					disabled={deleting}
				>
					<Pencil class="h-4 w-4" />
				</Button>
				<Button
					variant="outline"
					size="sm"
					onclick={() => onDelete(cashier.cashierId)}
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