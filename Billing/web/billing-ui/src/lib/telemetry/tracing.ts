import { trace } from '@opentelemetry/api';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-proto';
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { SimpleSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { BROWSER } from 'esm-env';
import type { TelemetryConfig } from './config';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from '@opentelemetry/semantic-conventions';

const DEFAULT_CONFIG: Required<TelemetryConfig> = {
	serviceName: 'billing-ui-frontend',
	serviceVersion: '1.0.0',
	otlpEndpoint: '/v1/traces',
	enabled: true,
	environment: 'development',
	otlpHeaders: '',
	otlpResourceAttributes: ''
};

let tracerProvider: WebTracerProvider | null = null;

/**
 * Initialize OpenTelemetry tracing for the frontend
 */
export function initializeTelemetry(config: TelemetryConfig = {}) {
	// Only initialize in browser environment
	if (!BROWSER) {
		return;
	}

	const finalConfig = { ...DEFAULT_CONFIG, ...config };

	if (!finalConfig.enabled) {
		console.log('OpenTelemetry tracing is disabled');
		return;
	}

	// Configure OTLP exporter
	const exporter = new OTLPTraceExporter({
		url: finalConfig.otlpEndpoint + '/v1/traces',
		headers: {
			...finalConfig.otlpHeaders.split(',').reduce(
				(acc, header) => {
					const [key, value] = header.split('=');
					acc[key.trim()] = value.trim();
					return acc;
				},
				{} as Record<string, string>
			)
		}
	});

	tracerProvider = new WebTracerProvider({
		resource: resourceFromAttributes({
			[ATTR_SERVICE_NAME]: finalConfig.serviceName,
			[ATTR_SERVICE_VERSION]: finalConfig.serviceVersion,
			...finalConfig.otlpResourceAttributes.split(',').reduce(
				(acc, attr) => {
					const [key, value] = attr.split('=');
					acc[key.trim()] = value.trim();
					return acc;
				},
				{} as Record<string, string>
			)
		}),
		spanProcessors: [new SimpleSpanProcessor(exporter)]
	});

	// Register the provider
	tracerProvider.register();

	// Register instrumentations
	registerInstrumentations({
		instrumentations: [
			new FetchInstrumentation({
				// Propagate trace headers to backend
				propagateTraceHeaderCorsUrls: [/^https?:\/\/localhost(:\d+)?\/api\//, /^\/api\//],
				// Ignore health checks and other noise
				ignoreUrls: [/\/health/, /\/favicon\.ico/]
			}),
			new DocumentLoadInstrumentation()
		]
	});

	console.log('🔍 OpenTelemetry Frontend Configuration:');
	console.log('  Service Name:', finalConfig.serviceName);
	console.log('  Environment:', finalConfig.environment);
	console.log('  OTLP Endpoint:', finalConfig.otlpEndpoint);
	console.log('  User Agent:', navigator.userAgent);
	console.log('  Language:', navigator.language);
	console.log('  Instrumentation: Fetch + Document Load enabled');
	console.log('  Trace Propagation: W3C TraceContext enabled');

	// Create a test span to validate tracing
	setTimeout(() => {
		const tracer = getTracer();
		tracer.startActiveSpan('TelemetryValidation.Frontend', (span) => {
			try {
				span.setAttributes({
					'validation.test': 'true',
					component: 'billing-ui-frontend',
					'page.url': window.location.href
				});
				console.log('✅ Frontend tracing validation successful!');
				console.log('  Test Trace ID:', span.spanContext().traceId);
				console.log('  Test Span ID:', span.spanContext().spanId);
				span.setStatus({ code: 1 }); // OK
			} catch (error) {
				console.error('❌ Frontend tracing validation failed:', error);
				span.recordException(error as Error);
				span.setStatus({ code: 2, message: 'Validation failed' });
			} finally {
				span.end();
			}
		});
	}, 1000); // Wait 1 second for everything to initialize
}

/**
 * Get the global tracer instance
 */
export function getTracer(name = 'billing-ui') {
	return trace.getTracer(name);
}

/**
 * Create a custom span for user interactions or business operations
 */
export function createSpan(name: string, fn: () => void | Promise<void>) {
	const tracer = getTracer();
	return tracer.startActiveSpan(name, async (span) => {
		try {
			await fn();
			span.setStatus({ code: 1 }); // OK
		} catch (error) {
			span.setStatus({
				code: 2, // ERROR
				message: error instanceof Error ? error.message : 'Unknown error'
			});
			span.recordException(error as Error);
			throw error;
		} finally {
			span.end();
		}
	});
}

/**
 * Shutdown telemetry gracefully
 */
export async function shutdownTelemetry() {
	if (tracerProvider) {
		await tracerProvider.shutdown();
		tracerProvider = null;
	}
}
