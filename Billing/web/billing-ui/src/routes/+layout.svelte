<script lang="ts">
	import '../app.css';
	import Button from '$lib/components/ui/button.svelte';
	import Card from '$lib/components/ui/card.svelte';
	import { AlertTriangle, RefreshCw } from 'lucide-svelte';

	interface ErrorInfo {
		message: string;
		stack?: string;
	}

	function handleError(error: unknown, reset: () => void) {
		console.error('Application error:', error);
		
		// You could send error to monitoring service here
		// analytics.captureException(error);
	}

	function reloadPage() {
		window.location.reload();
	}
</script>

<div class="relative flex min-h-screen flex-col bg-background">
	<!-- Skip link for accessibility -->
	<a 
		href="#main-content" 
		class="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:z-50 focus:px-4 focus:py-2 focus:bg-primary focus:text-primary-foreground focus:rounded-md focus:no-underline"
	>
		Skip to main content
	</a>
	
	<header class="sticky top-0 z-50 w-full border-b border-border/40 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
		<div class="container flex h-14 max-w-screen-2xl items-center">
			<nav class="flex items-center space-x-4 lg:space-x-6 mx-6">
				<a href="/" class="text-sm font-medium transition-colors hover:text-primary">
					Billing Service
				</a>
				<a href="/cashiers" class="text-sm font-medium text-muted-foreground transition-colors hover:text-primary">
					Cashiers
				</a>
			</nav>
		</div>
	</header>
	<main id="main-content" class="flex-1">
		<svelte:boundary onerror={handleError}>
			<slot />
			
			{#snippet failed(error, reset)}
				<div class="container mx-auto px-4 py-8">
					<Card class="p-8 max-w-md mx-auto">
						<div class="text-center space-y-4">
							<AlertTriangle class="h-12 w-12 mx-auto text-destructive" />
							<div class="space-y-2">
								<h2 class="text-lg font-semibold">Something went wrong</h2>
								<p class="text-sm text-muted-foreground">
									{(error as ErrorInfo)?.message || 'An unexpected error occurred'}
								</p>
							</div>
							<div class="flex gap-2 justify-center">
								<Button onclick={reset} variant="default">
									Try Again
								</Button>
								<Button onclick={reloadPage} variant="outline">
									<RefreshCw class="h-4 w-4 mr-2" />
									Reload Page
								</Button>
							</div>
						</div>
					</Card>
				</div>
			{/snippet}
		</svelte:boundary>
	</main>
</div>