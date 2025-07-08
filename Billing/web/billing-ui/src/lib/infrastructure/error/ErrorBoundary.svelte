<script lang="ts">
	import { onMount } from 'svelte';
	import { recordEvent } from '../telemetry/TelemetryService';
	import { ApiError } from '../api/ApiClient';

	let { children, fallback } = $props<{
		children: () => void;
		fallback?: (error: Error) => void;
	}>();

	let error = $state<Error | null>(null);
	let retryCount = $state(0);
	const maxRetries = 3;

	onMount(() => {
		const handleError = (event: ErrorEvent) => {
			error = event.error;
			recordEvent('error.boundary.triggered', {
				'error.message': event.error.message,
				'error.stack': event.error.stack?.substring(0, 1000) || '',
				'error.type': event.error.constructor.name,
				'retry.count': retryCount
			});
		};

		const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
			const err =
				event.reason instanceof Error ? event.reason : new Error(String(event.reason));
			error = err;
			recordEvent('error.boundary.unhandled_promise', {
				'error.message': err.message,
				'error.type': err.constructor.name,
				'retry.count': retryCount
			});
		};

		window.addEventListener('error', handleError);
		window.addEventListener('unhandledrejection', handleUnhandledRejection);

		return () => {
			window.removeEventListener('error', handleError);
			window.removeEventListener('unhandledrejection', handleUnhandledRejection);
		};
	});

	function retry() {
		if (retryCount < maxRetries) {
			retryCount++;
			error = null;
			recordEvent('error.boundary.retry_attempt', {
				'retry.count': retryCount
			});
		}
	}

	function reset() {
		error = null;
		retryCount = 0;
		recordEvent('error.boundary.reset');
	}

	function getErrorMessage(err: Error): string {
		if (err instanceof ApiError) {
			if (err.status === 0) {
				return 'Network connection failed. Please check your internet connection.';
			}
			if (err.status >= 500) {
				return 'Server error occurred. Please try again later.';
			}
			if (err.status === 404) {
				return 'The requested resource was not found.';
			}
			return err.message;
		}
		return err.message || 'An unexpected error occurred.';
	}

	function canRetry(err: Error): boolean {
		if (err instanceof ApiError) {
			// Retry on network errors or server errors
			return err.status === 0 || err.status >= 500;
		}
		// Don't retry on client errors
		return false;
	}
</script>

{#if error}
	{#if fallback}
		{@render fallback(error)}
	{:else}
		<div class="error-boundary rounded-lg border border-red-200 bg-red-50 p-6">
			<div class="flex items-start space-x-4">
				<div class="flex-shrink-0">
					<svg
						class="h-6 w-6 text-red-600"
						fill="none"
						stroke="currentColor"
						viewBox="0 0 24 24"
					>
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.5 0L4.268 19.5c-.77.833.192 2.5 1.732 2.5z"
						/>
					</svg>
				</div>
				<div class="flex-1">
					<h3 class="text-lg font-medium text-red-800">Something went wrong</h3>
					<p class="mt-2 text-sm text-red-700">{getErrorMessage(error)}</p>

					{#if canRetry(error) && retryCount < maxRetries}
						<div class="mt-4 flex space-x-3">
							<button
								onclick={retry}
								class="inline-flex items-center rounded-md border border-transparent bg-red-600 px-3 py-2 text-sm font-medium leading-4 text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
							>
								Retry ({maxRetries - retryCount} attempts left)
							</button>
							<button
								onclick={reset}
								class="inline-flex items-center rounded-md border border-red-300 bg-white px-3 py-2 text-sm font-medium leading-4 text-red-700 hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
							>
								Reset
							</button>
						</div>
					{:else}
						<div class="mt-4">
							<button
								onclick={reset}
								class="inline-flex items-center rounded-md border border-red-300 bg-white px-3 py-2 text-sm font-medium leading-4 text-red-700 hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
							>
								Reset
							</button>
						</div>
					{/if}

					{#if retryCount >= maxRetries}
						<p class="mt-2 text-xs text-red-600">
							Maximum retry attempts reached. Please refresh the page or contact
							support if the problem persists.
						</p>
					{/if}
				</div>
			</div>
		</div>
	{/if}
{:else}
	{@render children()}
{/if}
