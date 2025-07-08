import { ApiError } from '../api/ApiClient';
import { notificationService } from './NotificationService';
import { recordEvent } from '../telemetry/TelemetryService';

export interface ErrorContext {
	operation?: string;
	componentName?: string;
	userId?: string;
	metadata?: Record<string, any>;
}

export class ErrorHandler {
	private static instance: ErrorHandler | null = null;

	private constructor() {}

	static getInstance(): ErrorHandler {
		if (!ErrorHandler.instance) {
			ErrorHandler.instance = new ErrorHandler();
		}
		return ErrorHandler.instance;
	}

	handle(error: unknown, context?: ErrorContext): void {
		const normalizedError = this.normalizeError(error);
		const errorInfo = this.analyzeError(normalizedError, context);

		// Log to telemetry
		this.logToTelemetry(normalizedError, errorInfo, context);

		// Show user notification
		this.showUserNotification(normalizedError, errorInfo, context);

		// Log to console in development
		if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
			console.error('Error handled:', {
				error: normalizedError,
				context,
				errorInfo
			});
		}
	}

	private normalizeError(error: unknown): Error {
		if (error instanceof Error) {
			return error;
		}
		if (typeof error === 'string') {
			return new Error(error);
		}
		return new Error('Unknown error occurred');
	}

	private analyzeError(error: Error, context?: ErrorContext) {
		const info = {
			type: error.constructor.name,
			message: error.message,
			stack: error.stack,
			isRetryable: false,
			severity: 'error' as 'error' | 'warning' | 'info',
			userMessage: 'An unexpected error occurred',
			suggestedAction: null as string | null
		};

		if (error instanceof ApiError) {
			info.isRetryable = this.isRetryableApiError(error);
			info.userMessage = this.getApiErrorMessage(error);
			info.suggestedAction = this.getApiErrorAction(error);
			
			if (error.status >= 400 && error.status < 500) {
				info.severity = 'warning';
			}
		}

		return info;
	}

	private isRetryableApiError(error: ApiError): boolean {
		// Network errors and server errors are retryable
		return error.status === 0 || error.status >= 500;
	}

	private getApiErrorMessage(error: ApiError): string {
		switch (error.status) {
			case 0:
				return 'Connection failed. Please check your internet connection.';
			case 400:
				return 'Invalid request. Please check your input and try again.';
			case 401:
				return 'You are not authorized to perform this action.';
			case 403:
				return 'Access denied. You do not have permission for this operation.';
			case 404:
				return 'The requested resource was not found.';
			case 409:
				return 'Conflict occurred. The resource may have been modified by another user.';
			case 422:
				return 'Validation failed. Please check your input.';
			case 429:
				return 'Too many requests. Please wait a moment and try again.';
			case 500:
				return 'Server error occurred. Please try again later.';
			case 502:
				return 'Service temporarily unavailable. Please try again in a few minutes.';
			case 503:
				return 'Service is currently under maintenance. Please try again later.';
			default:
				return error.message || 'An error occurred while processing your request.';
		}
	}

	private getApiErrorAction(error: ApiError): string | null {
		switch (error.status) {
			case 0:
			case 500:
			case 502:
			case 503:
				return 'Retry';
			case 401:
				return 'Login again';
			case 429:
				return 'Wait and retry';
			default:
				return null;
		}
	}

	private logToTelemetry(error: Error, errorInfo: any, context?: ErrorContext): void {
		recordEvent('error.handled', {
			'error.type': errorInfo.type,
			'error.message': error.message,
			'error.severity': errorInfo.severity,
			'error.is_retryable': errorInfo.isRetryable,
			'error.context.operation': context?.operation || '',
			'error.context.component': context?.componentName || '',
			'error.stack': error.stack?.substring(0, 1000) || '',
			...(context?.metadata || {})
		});
	}

	private showUserNotification(error: Error, errorInfo: any, context?: ErrorContext): void {
		const title = this.getNotificationTitle(error, context);
		const action = errorInfo.suggestedAction ? {
			label: errorInfo.suggestedAction,
			handler: () => this.handleSuggestedAction(error, errorInfo.suggestedAction, context)
		} : undefined;

		if (errorInfo.severity === 'error') {
			notificationService.error(title, errorInfo.userMessage, { action });
		} else if (errorInfo.severity === 'warning') {
			notificationService.warning(title, errorInfo.userMessage, { action });
		} else {
			notificationService.info(title, errorInfo.userMessage, { action });
		}
	}

	private getNotificationTitle(error: Error, context?: ErrorContext): string {
		if (context?.operation) {
			return `${context.operation} failed`;
		}
		if (error instanceof ApiError) {
			return 'Request failed';
		}
		return 'Error occurred';
	}

	private handleSuggestedAction(error: Error, action: string, context?: ErrorContext): void {
		switch (action) {
			case 'Retry':
				if (context?.metadata?.retryHandler) {
					context.metadata.retryHandler();
				} else {
					// Default retry behavior - reload the page
					window.location.reload();
				}
				break;
			case 'Login again':
				// Redirect to login
				window.location.href = '/login';
				break;
			case 'Wait and retry':
				// Wait 5 seconds then retry
				setTimeout(() => {
					if (context?.metadata?.retryHandler) {
						context.metadata.retryHandler();
					}
				}, 5000);
				break;
		}
	}

	// Convenience methods
	handleApiError(error: ApiError, operation?: string, retryHandler?: () => void): void {
		this.handle(error, {
			operation,
			metadata: { retryHandler }
		});
	}

	handleFormError(error: unknown, formName?: string): void {
		this.handle(error, {
			operation: formName ? `Submit ${formName}` : 'Submit form',
			componentName: 'Form'
		});
	}

	handleLoadError(error: unknown, resourceName?: string): void {
		this.handle(error, {
			operation: resourceName ? `Load ${resourceName}` : 'Load data',
			componentName: 'DataLoader'
		});
	}
}

// Export singleton instance
export const errorHandler = ErrorHandler.getInstance();