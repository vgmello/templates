import { BROWSER } from 'esm-env';
import { initializeTelemetry } from './tracing';
import { telemetryConfig } from './config';

// Initialize telemetry when the module is imported on the client side
if (BROWSER) {
	initializeTelemetry(telemetryConfig);
}
