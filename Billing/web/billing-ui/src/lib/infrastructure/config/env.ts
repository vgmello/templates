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
	const environment = getEnvVar('PUBLIC_ENVIRONMENT', 'development');
	
	// Get API base URL with environment-specific defaults
	let apiBaseUrlDefault: string;
	switch (environment) {
		case 'production':
			// Require explicit configuration in production
			apiBaseUrlDefault = '';
			break;
		case 'staging':
			apiBaseUrlDefault = '';
			break;
		case 'development':
		default:
			apiBaseUrlDefault = 'http://localhost:8101';
			break;
	}
	
	const apiBaseUrl = getEnvVar('PUBLIC_API_BASE_URL', apiBaseUrlDefault);
	
	// Validate API base URL in production
	if (environment === 'production' && (!apiBaseUrl || apiBaseUrl.includes('localhost'))) {
		throw new Error('PUBLIC_API_BASE_URL must be explicitly set for production environment and cannot contain localhost');
	}
	
	// Validate API base URL format
	if (apiBaseUrl && !apiBaseUrl.match(/^https?:\/\/.+/)) {
		throw new Error(`Invalid API base URL format: ${apiBaseUrl}. Must start with http:// or https://`);
	}
	
	return {
		apiBaseUrl,
		telemetry: {
			enabled: getBooleanEnvVar('PUBLIC_OTEL_ENABLED', environment === 'development'),
			serviceName: getEnvVar('PUBLIC_OTEL_SERVICE_NAME', 'billing-ui'),
			serviceVersion: getEnvVar('PUBLIC_OTEL_SERVICE_VERSION', '1.0.0'),
			otlpEndpoint: getEnvVar('PUBLIC_OTEL_EXPORTER_OTLP_ENDPOINT', '/v1/traces'),
			environment,
			otlpHeaders: getEnvVar('PUBLIC_OTEL_EXPORTER_OTLP_HEADERS', ''),
			otlpResourceAttributes: getEnvVar('PUBLIC_OTEL_RESOURCE_ATTRIBUTES', '')
		},
		environment
	};
}

export const config = getConfig();
