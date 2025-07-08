import { BROWSER } from 'esm-env';
import { config } from '../config/env';

export class TelemetryService {
	private static instance: TelemetryService | null = null;
	private initialized = false;

	private constructor() {}

	static getInstance(): TelemetryService {
		if (!TelemetryService.instance) {
			TelemetryService.instance = new TelemetryService();
		}
		return TelemetryService.instance;
	}

	initialize(): void {
		// Only initialize in browser environment and if not already initialized
		if (!BROWSER || this.initialized || !config.telemetry.enabled) {
			if (!config.telemetry.enabled) {
				console.log('OpenTelemetry tracing is disabled');
			}
			return;
		}

		// Simplified telemetry initialization
		// TODO: Re-implement with OpenTelemetry when build issues are resolved
		this.initialized = true;
		console.log('Telemetry service initialized (simplified mode)');
	}

	getTracer(): unknown {
		// Return a mock tracer object
		return {
			startActiveSpan: (spanName: string, fn: (span: unknown) => unknown) => {
				// For now, just execute the function directly
				const mockSpan = {
					setAttributes: () => {},
					setStatus: () => {},
					addEvent: () => {},
					recordException: () => {},
					end: () => {},
					spanContext: () => ({ traceId: 'mock-trace', spanId: 'mock-span' })
				};
				return fn(mockSpan);
			}
		};
	}

	async createSpan<T>(name: string, fn: () => T | Promise<T>): Promise<T> {
		// Simplified implementation - just execute the function
		console.log(`[Telemetry] Starting span: ${name}`);
		try {
			const result = await fn();
			console.log(`[Telemetry] Span completed: ${name}`);
			return result;
		} catch (error) {
			console.log(`[Telemetry] Span failed: ${name}`, error);
			throw error;
		}
	}

	recordEvent(name: string, attributes?: Record<string, string | number | boolean>): void {
		console.log(`[Telemetry] Event: ${name}`, attributes);
	}

	async shutdown(): Promise<void> {
		this.initialized = false;
		console.log('Telemetry service shutdown');
	}
}

// Create and export singleton instance
export const telemetryService = TelemetryService.getInstance();

// Export convenience functions
export const getTracer = () => telemetryService.getTracer();
export const createSpan = <T>(name: string, fn: () => T | Promise<T>) =>
	telemetryService.createSpan(name, fn);
export const recordEvent = (name: string, attributes?: Record<string, string | number | boolean>) =>
	telemetryService.recordEvent(name, attributes);

// Decorator for automatic span creation
export function traced(spanName?: string) {
	return function <T extends (...args: unknown[]) => unknown>(
		_target: unknown,
		_propertyKey: string,
		descriptor: TypedPropertyDescriptor<T>
	) {
		const originalMethod = descriptor.value!;
		const finalSpanName = spanName || 'traced-method';

		descriptor.value = function (this: unknown, ...args: unknown[]) {
			return telemetryService.createSpan(finalSpanName, () =>
				originalMethod.apply(this, args)
			);
		} as T;

		return descriptor;
	};
}
