// Configuration
export { config, getConfig, type AppConfig } from './config/env';

// API Client
export { ApiClient, ApiError, apiClient, type ApiRequestOptions } from './api/ApiClient';

// Telemetry
export { 
	telemetryService, 
	TelemetryService, 
	getTracer, 
	createSpan, 
	recordEvent, 
	traced 
} from './telemetry/TelemetryService';

// Error Handling
export { errorHandler, ErrorHandler, type ErrorContext } from './error/ErrorHandler';
export { notificationService, NotificationService, type Notification } from './error/NotificationService';
export { default as ErrorBoundary } from './error/ErrorBoundary.svelte';
export { default as NotificationDisplay } from './error/NotificationDisplay.svelte';