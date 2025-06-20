<script>
	import '../app.css';
</script>

<div class="relative flex min-h-screen flex-col bg-background">
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
	<main class="flex-1">
		<slot />
	</main>
</div>