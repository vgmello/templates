<script lang="ts">
	import '../app.css';
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import { Button } from '$lib/ui/button';
	import { Users, Home, FileText } from '@lucide/svelte';
	import { telemetryService, ErrorBoundary, NotificationDisplay } from '$lib/infrastructure';

	let { children } = $props();

	const navigation = [
		{ name: 'Home', href: '/', icon: Home },
		{ name: 'Cashiers', href: '/cashiers', icon: Users },
		{ name: 'Invoices', href: '/invoices', icon: FileText }
	];

	onMount(() => {
		// Initialize telemetry
		telemetryService.initialize();
	});
</script>

<!-- Error Boundary wraps the entire app -->
<ErrorBoundary>
	<div class="min-h-screen bg-background">
		<!-- Navigation Header -->
		<header
			class="border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60"
		>
			<div class="container mx-auto px-6 py-4">
				<div class="flex items-center justify-between">
					<div class="flex items-center space-x-6">
						<a href="/" class="flex items-center space-x-2">
							<div
								class="flex h-8 w-8 items-center justify-center rounded-md bg-primary"
							>
								<span class="text-sm font-bold text-primary-foreground">B</span>
							</div>
							<span class="text-xl font-bold">Billing System</span>
						</a>

						<nav class="hidden items-center space-x-1 md:flex">
							{#each navigation as item (item.href)}
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

	<!-- Notification Display -->
	<NotificationDisplay />
</ErrorBoundary>
