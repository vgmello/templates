import { drizzle } from 'drizzle-orm/postgres-js';
import postgres from 'postgres';
import * as schema from './schema';
import { env } from '$env/dynamic/private';
import { building, dev } from '$app/environment';

// Mock database for cashier management UI (we use REST API instead)
const mockDb = {
	insert: () => ({ values: () => Promise.resolve({}) }),
	select: () => ({ from: () => ({ innerJoin: () => ({ where: () => [] }) }) }),
	delete: () => ({ where: () => Promise.resolve() }),
	update: () => ({ set: () => ({ where: () => Promise.resolve() }) })
} as any;

// Don't connect to database during build time or development (use REST API instead)
let db: any;

if (building || dev) {
	// Use mock db for development and build
	db = mockDb;
} else {
	// Only connect to real database in production if needed
	try {
		const databaseUrl = env.DATABASE_URL;
		if (databaseUrl) {
			const client = postgres(databaseUrl);
			db = drizzle(client, { schema });
		} else {
			db = mockDb;
		}
	} catch {
		db = mockDb;
	}
}

export { db };
