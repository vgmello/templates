export interface Notification {
	id: string;
	type: 'success' | 'error' | 'warning' | 'info';
	title: string;
	message?: string;
	duration?: number;
	action?: {
		label: string;
		handler: () => void;
	};
	timestamp: Date;
}

export class NotificationService {
	private static instance: NotificationService | null = null;
	private notifications: Notification[] = [];

	private constructor() {}

	static getInstance(): NotificationService {
		if (!NotificationService.instance) {
			NotificationService.instance = new NotificationService();
		}
		return NotificationService.instance;
	}

	getNotifications(): Notification[] {
		return this.notifications;
	}

	private generateId(): string {
		return `notification-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
	}

	add(notification: Omit<Notification, 'id' | 'timestamp'>): string {
		const id = this.generateId();
		const newNotification: Notification = {
			...notification,
			id,
			timestamp: new Date()
		};

		this.notifications = [...this.notifications, newNotification];

		// Auto-remove notification after duration
		if (notification.duration !== 0) {
			const duration = notification.duration || this.getDefaultDuration(notification.type);
			setTimeout(() => {
				this.remove(id);
			}, duration);
		}

		return id;
	}

	remove(id: string): void {
		this.notifications = this.notifications.filter((n) => n.id !== id);
	}

	clear(): void {
		this.notifications = [];
	}

	success(title: string, message?: string, options?: Partial<Notification>): string {
		return this.add({
			type: 'success',
			title,
			message,
			...options
		});
	}

	error(title: string, message?: string, options?: Partial<Notification>): string {
		return this.add({
			type: 'error',
			title,
			message,
			duration: 0, // Don't auto-remove error messages
			...options
		});
	}

	warning(title: string, message?: string, options?: Partial<Notification>): string {
		return this.add({
			type: 'warning',
			title,
			message,
			...options
		});
	}

	info(title: string, message?: string, options?: Partial<Notification>): string {
		return this.add({
			type: 'info',
			title,
			message,
			...options
		});
	}

	private getDefaultDuration(type: Notification['type']): number {
		switch (type) {
			case 'success':
				return 5000;
			case 'info':
				return 7000;
			case 'warning':
				return 10000;
			case 'error':
				return 0; // Don't auto-remove errors
			default:
				return 5000;
		}
	}
}

// Export singleton instance
export const notificationService = NotificationService.getInstance();
