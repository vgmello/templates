import { dev } from '$app/environment';
import { env } from '$env/dynamic/public';

export interface TelemetryConfig {
	serviceName?: string;
	serviceVersion?: string;
	otlpEndpoint?: string;
	otlpHeaders?: string;
	otlpResourceAttributes?: string;
	enabled?: boolean;
	environment?: string;
}

export const telemetryConfig: TelemetryConfig = {
	enabled: true,
	serviceName: env.PUBLIC_OTEL_SERVICE_NAME ?? 'billing-ui',
	serviceVersion: '1.0.0',
	environment: dev ? 'development' : 'production',
	otlpEndpoint: env.PUBLIC_OTEL_EXPORTER_OTLP_ENDPOINT,
	otlpHeaders: env.PUBLIC_OTEL_EXPORTER_OTLP_HEADERS,
	otlpResourceAttributes: env.PUBLIC_OTEL_RESOURCE_ATTRIBUTES
};
