<script lang="ts">
	import { cn } from '$lib/utils.js';
	import { ChevronDown, Check } from '@lucide/svelte';

	type Option = {
		value: string | undefined;
		label: string;
		disabled?: boolean;
	};

	type Props = {
		id?: string;
		value: string | undefined;
		options: Option[];
		placeholder?: string;
		disabled?: boolean;
		error?: string;
		class?: string;
		onchange?: (value: string | undefined) => void;
	};

	let {
		id,
		value = $bindable(),
		options,
		placeholder = "Select an option...",
		disabled = false,
		error,
		class: className,
		onchange
	}: Props = $props();

	let isOpen = $state(false);
	let selectElement: HTMLElement;

	const selectedOption = $derived(options.find(option => option.value === value));

	function handleSelect(option: Option) {
		if (option.disabled) return;
		
		value = option.value;
		isOpen = false;
		onchange?.(option.value);
	}

	function handleKeydown(event: KeyboardEvent) {
		if (disabled) return;

		switch (event.key) {
			case 'Enter':
			case ' ':
				event.preventDefault();
				isOpen = !isOpen;
				break;
			case 'Escape':
				isOpen = false;
				break;
			case 'ArrowDown':
				event.preventDefault();
				if (!isOpen) {
					isOpen = true;
				} else {
					// Focus next option logic could be added here
				}
				break;
			case 'ArrowUp':
				event.preventDefault();
				if (isOpen) {
					// Focus previous option logic could be added here
				}
				break;
		}
	}

	function handleClickOutside(event: MouseEvent) {
		if (selectElement && !selectElement.contains(event.target as Node)) {
			isOpen = false;
		}
	}

	$effect(() => {
		if (typeof document !== 'undefined') {
			document.addEventListener('click', handleClickOutside);
			return () => document.removeEventListener('click', handleClickOutside);
		}
	});
</script>

<div bind:this={selectElement} class="relative w-full">
	<button
		{id}
		type="button"
		class={cn(
			"flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
			error && "border-destructive",
			className
		)}
		aria-haspopup="listbox"
		aria-expanded={isOpen}
		{disabled}
		onclick={() => !disabled && (isOpen = !isOpen)}
		onkeydown={handleKeydown}
	>
		<span class={cn("block truncate", !selectedOption && "text-muted-foreground")}>
			{selectedOption?.label || placeholder}
		</span>
		<ChevronDown 
			class={cn(
				"h-4 w-4 transition-transform duration-200",
				isOpen && "rotate-180"
			)} 
		/>
	</button>

	{#if isOpen}
		<div
			class="absolute z-50 mt-1 w-full rounded-md border bg-popover shadow-lg"
			role="listbox"
		>
			<div class="max-h-60 overflow-auto p-1">
				{#each options as option}
					<button
						type="button"
						class={cn(
							"relative flex w-full cursor-pointer select-none items-center rounded-sm py-2 pl-8 pr-2 text-sm outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground",
							option.disabled && "cursor-not-allowed opacity-50",
							value === option.value && "bg-accent text-accent-foreground"
						)}
						disabled={option.disabled}
						onclick={() => handleSelect(option)}
						role="option"
						aria-selected={value === option.value}
					>
						{#if value === option.value}
							<span class="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
								<Check class="h-4 w-4" />
							</span>
						{/if}
						<span class="block truncate">
							{option.label}
						</span>
					</button>
				{/each}
			</div>
		</div>
	{/if}

	{#if error}
		<p class="mt-1 text-sm text-destructive">{error}</p>
	{/if}
</div>