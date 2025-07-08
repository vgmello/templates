<script lang="ts">
	import { notificationService, type Notification } from './NotificationService';
	import { fly } from 'svelte/transition';

	const notifications = $derived(notificationService.getNotifications());

	function getIcon(type: Notification['type']) {
		switch (type) {
			case 'success':
				return `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />`;
			case 'error':
				return `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />`;
			case 'warning':
				return `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.5 0L4.268 19.5c-.77.833.192 2.5 1.732 2.5z" />`;
			case 'info':
				return `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />`;
		}
	}

	function getColors(type: Notification['type']) {
		switch (type) {
			case 'success':
				return {
					bg: 'bg-green-50',
					border: 'border-green-200',
					icon: 'text-green-400',
					title: 'text-green-800',
					message: 'text-green-700',
					close: 'text-green-400 hover:text-green-600',
					action: 'text-green-600 hover:text-green-800'
				};
			case 'error':
				return {
					bg: 'bg-red-50',
					border: 'border-red-200',
					icon: 'text-red-400',
					title: 'text-red-800',
					message: 'text-red-700',
					close: 'text-red-400 hover:text-red-600',
					action: 'text-red-600 hover:text-red-800'
				};
			case 'warning':
				return {
					bg: 'bg-yellow-50',
					border: 'border-yellow-200',
					icon: 'text-yellow-400',
					title: 'text-yellow-800',
					message: 'text-yellow-700',
					close: 'text-yellow-400 hover:text-yellow-600',
					action: 'text-yellow-600 hover:text-yellow-800'
				};
			case 'info':
				return {
					bg: 'bg-blue-50',
					border: 'border-blue-200',
					icon: 'text-blue-400',
					title: 'text-blue-800',
					message: 'text-blue-700',
					close: 'text-blue-400 hover:text-blue-600',
					action: 'text-blue-600 hover:text-blue-800'
				};
		}
	}

	function formatTime(date: Date): string {
		return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
	}
</script>

<div class="fixed top-4 right-4 z-50 space-y-4 max-w-sm w-full">
	{#each notifications as notification (notification.id)}
		{@const colors = getColors(notification.type)}
		<div
			class="notification-item {colors.bg} {colors.border} border rounded-lg shadow-lg p-4"
			transition:fly={{ x: 300, duration: 300 }}
		>
			<div class="flex items-start">
				<div class="flex-shrink-0">
					<svg class="w-5 h-5 {colors.icon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						{@html getIcon(notification.type)}
					</svg>
				</div>
				
				<div class="ml-3 flex-1">
					<div class="flex items-center justify-between">
						<h4 class="text-sm font-medium {colors.title}">
							{notification.title}
						</h4>
						<div class="flex items-center space-x-2">
							<span class="text-xs {colors.message}">
								{formatTime(notification.timestamp)}
							</span>
							<button
								onclick={() => notificationService.remove(notification.id)}
								class="inline-flex {colors.close} hover:bg-white hover:bg-opacity-20 rounded-md p-1.5 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-transparent focus:ring-white"
							>
								<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
								</svg>
							</button>
						</div>
					</div>
					
					{#if notification.message}
						<p class="mt-1 text-sm {colors.message}">
							{notification.message}
						</p>
					{/if}
					
					{#if notification.action}
						<div class="mt-3">
							<button
								onclick={notification.action.handler}
								class="text-sm {colors.action} font-medium hover:underline focus:outline-none focus:underline"
							>
								{notification.action.label}
							</button>
						</div>
					{/if}
				</div>
			</div>
		</div>
	{/each}
</div>

<style>
	.notification-item {
		animation: slideIn 0.3s ease-out;
	}

	@keyframes slideIn {
		from {
			transform: translateX(100%);
			opacity: 0;
		}
		to {
			transform: translateX(0);
			opacity: 1;
		}
	}
</style>