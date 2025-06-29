import { trace, context, propagation, logs } from '@opentelemetry/api';
import { WebSDK } from '@opentelemetry/sdk-trace-web';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { OTLPTraceExporter } from '@opentelemetry/exporter-otlp-http';
import { Resource } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from '@opentelemetry/semantic-conventions';
import { browser } from '$app/environment';

let sdk: WebSDK | null = null;

export function initializeTelemetry() {
	if (!browser) return;
	
	// Get OTLP endpoint from environment or default to Aspire
	const otlpEndpoint = (typeof window !== 'undefined' && window.location) 
		? `${window.location.protocol}//${window.location.hostname}:18110/v1/traces`
		: 'http://localhost:18110/v1/traces';
	
	// Check for environment variable override
	const envEndpoint = import.meta.env.VITE_OTEL_EXPORTER_OTLP_ENDPOINT;
	const endpoint = envEndpoint || otlpEndpoint;

	console.log('Initializing OpenTelemetry with endpoint:', endpoint);

	// Configure resource
	const resource = new Resource({
		[ATTR_SERVICE_NAME]: 'billing-ui',
		[ATTR_SERVICE_VERSION]: '1.0.0',
	});

	// Configure OTLP exporter
	const exporter = new OTLPTraceExporter({
		url: endpoint,
		headers: {},
	});

	// Initialize SDK
	sdk = new WebSDK({
		resource,
		spanProcessors: [
			new trace.BatchSpanProcessor(exporter)
		],
		contextManager: new ZoneContextManager(),
		instrumentations: [
			new DocumentLoadInstrumentation(),
		],
	});

	// Start the SDK
	sdk.start();
	
	console.log('OpenTelemetry initialized successfully');
}

export function getTracer(name: string = 'billing-ui') {
	return trace.getTracer(name);
}

export function createSpan(tracer: any, name: string, parentContext?: any) {
	const ctx = parentContext || context.active();
	return tracer.startSpan(name, {}, ctx);
}

export function injectHeaders(headers: Record<string, string> = {}): Record<string, string> {
	const carrier: Record<string, string> = { ...headers };
	propagation.inject(context.active(), carrier);
	return carrier;
}

export function extractContext(headers: Record<string, string>) {
	return propagation.extract(context.active(), headers);
}

export function withSpan<T>(tracer: any, name: string, fn: (span: any) => T): T {
	const span = tracer.startSpan(name);
	try {
		return context.with(trace.setSpan(context.active(), span), () => {
			const result = fn(span);
			
			// Handle promises
			if (result && typeof result === 'object' && 'then' in result) {
				return (result as Promise<any>)
					.then((value) => {
						span.setStatus({ code: trace.SpanStatusCode.OK });
						span.end();
						return value;
					})
					.catch((error) => {
						span.recordException(error);
						span.setStatus({
							code: trace.SpanStatusCode.ERROR,
							message: error.message,
						});
						span.end();
						throw error;
					});
			}
			
			// Handle synchronous results
			span.setStatus({ code: trace.SpanStatusCode.OK });
			span.end();
			return result;
		});
	} catch (error) {
		span.recordException(error as Error);
		span.setStatus({
			code: trace.SpanStatusCode.ERROR,
			message: (error as Error).message,
		});
		span.end();
		throw error;
	}
}

// Custom console wrapper that sends logs to OpenTelemetry
const originalConsole = {
	log: console.log,
	error: console.error,
	warn: console.warn,
	info: console.info,
	debug: console.debug
};

export function enableOpenTelemetryLogging() {
	if (!browser) return;

	const logger = logs.getLogger('billing-ui', '1.0.0');

	// Override console methods to send logs to OpenTelemetry
	console.log = (...args: any[]) => {
		originalConsole.log(...args);
		logger.emit({
			severityText: 'INFO',
			body: args.join(' '),
			timestamp: Date.now(),
			attributes: {
				'log.level': 'info',
				'service.name': 'billing-ui'
			}
		});
	};

	console.error = (...args: any[]) => {
		originalConsole.error(...args);
		logger.emit({
			severityText: 'ERROR',
			body: args.join(' '),
			timestamp: Date.now(),
			attributes: {
				'log.level': 'error',
				'service.name': 'billing-ui'
			}
		});
	};

	console.warn = (...args: any[]) => {
		originalConsole.warn(...args);
		logger.emit({
			severityText: 'WARN',
			body: args.join(' '),
			timestamp: Date.now(),
			attributes: {
				'log.level': 'warn',
				'service.name': 'billing-ui'
			}
		});
	};

	console.info = (...args: any[]) => {
		originalConsole.info(...args);
		logger.emit({
			severityText: 'INFO',
			body: args.join(' '),
			timestamp: Date.now(),
			attributes: {
				'log.level': 'info',
				'service.name': 'billing-ui'
			}
		});
	};

	console.debug = (...args: any[]) => {
		originalConsole.debug(...args);
		logger.emit({
			severityText: 'DEBUG',
			body: args.join(' '),
			timestamp: Date.now(),
			attributes: {
				'log.level': 'debug',
				'service.name': 'billing-ui'
			}
		});
	};
}

export function disableOpenTelemetryLogging() {
	console.log = originalConsole.log;
	console.error = originalConsole.error;
	console.warn = originalConsole.warn;
	console.info = originalConsole.info;
	console.debug = originalConsole.debug;
}

export function shutdown() {
	disableOpenTelemetryLogging();
	if (sdk) {
		sdk.shutdown();
		sdk = null;
	}
}