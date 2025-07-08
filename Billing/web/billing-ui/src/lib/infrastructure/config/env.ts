import { env } from '$env/dynamic/public';

export interface AppConfig {
	apiBaseUrl: string;
	telemetry: {
		enabled: boolean;
		serviceName: string;
		serviceVersion: string;
		otlpEndpoint: string;
		environment: string;
		otlpHeaders: string;
		otlpResourceAttributes: string;
	};
	environment: string;
}

function getEnvVar(key: string, defaultValue: string = ''): string {
	return (env as Record<string, string | undefined>)[key] || defaultValue;
}

function getBooleanEnvVar(key: string, defaultValue: boolean = false): boolean {
	const value = getEnvVar(key, defaultValue.toString()).toLowerCase();
	return value === 'true' || value === '1' || value === 'yes';
}

export function getConfig(): AppConfig {
	return {
		apiBaseUrl: getEnvVar('PUBLIC_API_BASE_URL', 'http://localhost:8101'),
		telemetry: {
			enabled: getBooleanEnvVar('PUBLIC_OTEL_ENABLED', true),
			serviceName: getEnvVar('PUBLIC_OTEL_SERVICE_NAME', 'billing-ui'),
			serviceVersion: getEnvVar('PUBLIC_OTEL_SERVICE_VERSION', '1.0.0'),
			otlpEndpoint: getEnvVar('PUBLIC_OTEL_EXPORTER_OTLP_ENDPOINT', '/v1/traces'),
			environment: getEnvVar('PUBLIC_ENVIRONMENT', 'development'),
			otlpHeaders: getEnvVar('PUBLIC_OTEL_EXPORTER_OTLP_HEADERS', ''),
			otlpResourceAttributes: getEnvVar('PUBLIC_OTEL_RESOURCE_ATTRIBUTES', '')
		},
		environment: getEnvVar('PUBLIC_ENVIRONMENT', 'development')
	};
}

export const config = getConfig();
