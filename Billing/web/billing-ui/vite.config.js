import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import devtoolsJson from 'vite-plugin-devtools-json';

export default defineConfig({
	plugins: [
		sveltekit(),
		devtoolsJson()
	],
	server: {
		port: process.env.PORT ? parseInt(process.env.PORT) : 8105,
		host: '0.0.0.0', // Allow access from other containers/services
		strictPort: true, // Fail if port is already in use
		hmr: {
			port: 8106 // Use different port for HMR to avoid conflicts
		}
	}
});