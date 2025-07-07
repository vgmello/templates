import type { Handle } from '@sveltejs/kit';
import { trace, SpanStatusCode } from '@opentelemetry/api';

// Initialize tracer for server-side operations
const tracer = trace.getTracer('billing-ui-server', '1.0.0');

// Handle OpenTelemetry tracing
const handleTracing: Handle = async ({ event, resolve }) => {
	const { method, url: requestUrl } = event.request;
	const url = new URL(requestUrl);
	const spanName = `${method} ${event.route?.id || url.pathname}`;

	// Start a new span for this request
	return tracer.startActiveSpan(spanName, async (span) => {
		try {
			// Add span attributes
			span.setAttributes({
				'http.method': method,
				'http.url': url.href,
				'http.target': url.pathname,
				'http.host': url.host,
				'http.scheme': url.protocol.replace(':', ''),
				'http.user_agent': event.request.headers.get('user-agent') || '',
				'route.id': event.route?.id || 'unknown'
			});

			// Store trace context in locals for use in load functions and actions
			event.locals.traceContext = {
				traceId: span.spanContext().traceId,
				spanId: span.spanContext().spanId,
				traceFlags: span.spanContext().traceFlags
			};

			// Log trace information for validation (only in development)
			if (event.url.searchParams.has('debug-trace')) {
				console.log('🔍 SvelteKit Server Trace Info:', {
					method,
					path: url.pathname,
					traceId: span.spanContext().traceId,
					spanId: span.spanContext().spanId,
					route: event.route?.id
				});
			}

			const response = await resolve(event);

			// Add response attributes
			span.setAttributes({
				'http.status_code': response.status,
				'http.response.size': response.headers.get('content-length') || '0'
			});

			// Set span status based on HTTP status
			if (response.status >= 400) {
				span.setStatus({
					code: SpanStatusCode.ERROR,
					message: `HTTP ${response.status}`
				});
			} else {
				span.setStatus({ code: SpanStatusCode.OK });
			}

			return response;
		} catch (error) {
			// Record exception and set error status
			span.recordException(error as Error);
			span.setStatus({
				code: SpanStatusCode.ERROR,
				message: error instanceof Error ? error.message : 'Unknown error'
			});
			throw error;
		} finally {
			span.end();
		}
	});
};

// Disabled auth for cashier management UI demo
// We're using REST API authentication instead of database sessions
const handleAuth: Handle = async ({ event, resolve }) => {
	// Skip auth validation for now - focus on cashier management
	event.locals.user = null;
	event.locals.session = null;
	return resolve(event);
};

export const handle: Handle = async ({ event, resolve }) => {
	// Chain multiple handlers
	return handleTracing({ event, resolve: (event) => handleAuth({ event, resolve }) });
};
