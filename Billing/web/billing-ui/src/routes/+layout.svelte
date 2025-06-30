<script lang="ts">
	import '../app.css';

	let { children } = $props();
</script>

<div class="min-h-screen bg-gray-50">
	<!-- Navigation -->
	<nav class="bg-white shadow-sm border-b border-gray-200">
		<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
			<div class="flex justify-between h-16">
				<div class="flex">
					<div class="flex-shrink-0 flex items-center">
						<h1 class="text-xl font-semibold text-gray-900">Billing Management</h1>
					</div>
					<div class="hidden sm:ml-6 sm:flex sm:space-x-8">
						<!-- Navigation items -->
						<a
							href="/"
							class="border-indigo-500 text-gray-900 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
							>Home</a
						>
						<a
							href="/cashiers"
							class="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
							>Cashiers</a
						>
						<a
							href="/demo"
							class="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
							>Demo</a
						>
					</div>
				</div>
			</div>
		</div>
	</nav>

	<!-- Main content -->
	<main>
		{@render children()}
	</main>
</div>
