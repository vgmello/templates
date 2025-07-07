<script lang="ts">
	import '../app.css';
	import { page } from '$app/stores';
	import { Button } from '$lib/components/ui/button';
	import { Users, Home, FileText } from '@lucide/svelte';

	let { children } = $props();
	
	const navigation = [
		{ name: 'Home', href: '/', icon: Home },
		{ name: 'Cashiers', href: '/cashiers', icon: Users },
		{ name: 'Invoices', href: '/invoices', icon: FileText }
	];
</script>

<div class="min-h-screen bg-background">
	<!-- Navigation Header -->
	<header class="border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
		<div class="container mx-auto px-6 py-4">
			<div class="flex items-center justify-between">
				<div class="flex items-center space-x-6">
					<a href="/" class="flex items-center space-x-2">
						<div class="h-8 w-8 bg-primary rounded-md flex items-center justify-center">
							<span class="text-primary-foreground font-bold text-sm">B</span>
						</div>
						<span class="font-bold text-xl">Billing System</span>
					</a>
					
					<nav class="hidden md:flex items-center space-x-1">
						{#each navigation as item}
							<Button
								variant={$page.url.pathname === item.href ? 'default' : 'ghost'}
								size="sm"
								class="flex items-center gap-2"
								href={item.href}
							>
								{@const Icon = item.icon}
								<Icon size={16} />
								{item.name}
							</Button>
						{/each}
					</nav>
				</div>
			</div>
		</div>
	</header>

	<!-- Main Content -->
	<main>
		{@render children()}
	</main>
</div>
