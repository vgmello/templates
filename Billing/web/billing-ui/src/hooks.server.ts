import type { Handle } from '@sveltejs/kit';

// Disabled auth for cashier management UI demo
// We're using REST API authentication instead of database sessions
const handleAuth: Handle = async ({ event, resolve }) => {
	// Skip auth validation for now - focus on cashier management
	event.locals.user = null;
	event.locals.session = null;
	return resolve(event);
};

export const handle: Handle = handleAuth;
