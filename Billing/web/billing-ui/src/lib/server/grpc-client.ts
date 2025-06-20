import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { env } from '$env/dynamic/private';
import { dev } from '$app/environment';
import type { Cashier, CreateCashierRequest, UpdateCashierRequest } from '../../app';

// Configuration
const GRPC_HOST = env.GRPC_HOST || 'localhost';
const GRPC_PORT = env.GRPC_PORT || '5061';
const PROTO_PATH = env.PROTO_PATH || './protos/billing.proto';

// gRPC client interface
interface GrpcCallback<T = any> {
	(error: grpc.ServiceError | null, response?: T): void;
}

interface GrpcClient {
	getCashiers: (request: {}, callback: GrpcCallback<{ cashiers: Cashier[] }>) => void;
	getCashier: (request: { cashierId: string }, callback: GrpcCallback<Cashier>) => void;
	createCashier: (request: CreateCashierRequest, callback: GrpcCallback<Cashier>) => void;
	updateCashier: (request: { cashierId: string } & UpdateCashierRequest, callback: GrpcCallback<Cashier>) => void;
	deleteCashier: (request: { cashierId: string }, callback: GrpcCallback<{}>) => void;
	close?: () => void;
}

// Error class for gRPC errors
export class GrpcError extends Error {
	code: number;
	details?: string;
	
	constructor(message: string, code: number, details?: string) {
		super(message);
		this.name = 'GrpcError';
		this.code = code;
		this.details = details;
	}
}

let client: GrpcClient | null = null;

/**
 * Initialize gRPC client
 */
async function initializeClient(): Promise<GrpcClient> {
	if (client) return client;

	try {
		// In development, we might not have the proto file, so we'll mock it
		if (dev && !env.PROTO_PATH) {
			console.warn('gRPC proto path not configured, using mock client');
			return createMockClient();
		}

		// Load proto definition
		const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
			keepCase: true,
			longs: String,
			enums: String,
			defaults: true,
			oneofs: true,
		});

		const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
		const BillingService = protoDescriptor.billing.BillingService;

		// Create client
		client = new BillingService(
			`${GRPC_HOST}:${GRPC_PORT}`,
			grpc.credentials.createInsecure()
		) as GrpcClient;

		return client;
	} catch (error) {
		console.error('Failed to initialize gRPC client:', error);
		
		// Fallback to mock client in development
		if (dev) {
			console.warn('Falling back to mock gRPC client');
			return createMockClient();
		}
		
		throw new GrpcError('Failed to initialize gRPC client', grpc.status.UNAVAILABLE, (error as Error).message);
	}
}

/**
 * Create a mock client for development/testing
 */
function createMockClient(): GrpcClient {
	const mockCashiers: Cashier[] = [
		{
			cashierId: "a52757cd-a42f-4fb9-8566-a98c61a71d2a",
			name: "Test Cashier",
			email: "test@example.com",
			cashierPayments: [
				{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() },
				{ currency: "EUR", isActive: true, createdDateUtc: new Date().toISOString() }
			],
			createdDateUtc: new Date().toISOString(),
			updatedDateUtc: new Date().toISOString(),
			version: 1
		},
		{
			cashierId: "b52757cd-a42f-4fb9-8566-a98c61a71d2a",
			name: "John Doe", 
			email: "john.doe@example.com",
			cashierPayments: [
				{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() }
			],
			createdDateUtc: new Date().toISOString(),
			updatedDateUtc: new Date().toISOString(),
			version: 1
		}
	];

	return {
		getCashiers: (request: {}, callback: GrpcCallback<{ cashiers: Cashier[] }>) => {
			setTimeout(() => {
				callback(null, { cashiers: mockCashiers });
			}, 100);
		},
		
		getCashier: (request: { cashierId: string }, callback: GrpcCallback<Cashier>) => {
			setTimeout(() => {
				const cashier = mockCashiers.find(c => c.cashierId === request.cashierId);
				if (cashier) {
					callback(null, cashier);
				} else {
					const error = new Error('Cashier not found') as grpc.ServiceError;
					error.code = grpc.status.NOT_FOUND;
					error.details = 'Cashier not found';
					error.metadata = new grpc.Metadata();
					callback(error);
				}
			}, 100);
		},
		
		createCashier: (request: CreateCashierRequest, callback: GrpcCallback<Cashier>) => {
			setTimeout(() => {
				const newCashier: Cashier = {
					cashierId: crypto.randomUUID(),
					name: request.name,
					email: request.email,
					cashierPayments: request.currencies?.map(currency => ({
						currency,
						isActive: true,
						createdDateUtc: new Date().toISOString()
					})) || [],
					createdDateUtc: new Date().toISOString(),
					updatedDateUtc: new Date().toISOString(),
					version: 1
				};
				mockCashiers.push(newCashier);
				callback(null, newCashier);
			}, 200);
		},
		
		updateCashier: (request: { cashierId: string } & UpdateCashierRequest, callback: GrpcCallback<Cashier>) => {
			setTimeout(() => {
				const index = mockCashiers.findIndex(c => c.cashierId === request.cashierId);
				if (index !== -1) {
					const updatedCashier: Cashier = {
						...mockCashiers[index],
						...request,
						updatedDateUtc: new Date().toISOString(),
						version: mockCashiers[index].version + 1
					};
					mockCashiers[index] = updatedCashier;
					callback(null, updatedCashier);
				} else {
					const error = new Error('Cashier not found') as grpc.ServiceError;
					error.code = grpc.status.NOT_FOUND;
					error.details = 'Cashier not found';
					error.metadata = new grpc.Metadata();
					callback(error);
				}
			}, 200);
		},
		
		deleteCashier: (request: { cashierId: string }, callback: GrpcCallback<{}>) => {
			setTimeout(() => {
				const index = mockCashiers.findIndex(c => c.cashierId === request.cashierId);
				if (index !== -1) {
					mockCashiers.splice(index, 1);
					callback(null, {});
				} else {
					const error = new Error('Cashier not found') as grpc.ServiceError;
					error.code = grpc.status.NOT_FOUND;
					error.details = 'Cashier not found';
					error.metadata = new grpc.Metadata();
					callback(error);
				}
			}, 200);
		}
	};
}

/**
 * Promisify gRPC call
 */
function promisifyCall<T>(client: GrpcClient, methodName: keyof GrpcClient, request: any): Promise<T> {
	return new Promise((resolve, reject) => {
		(client[methodName] as any)(request, (error: grpc.ServiceError | null, response?: T) => {
			if (error) {
				reject(new GrpcError(
					error.message || 'gRPC call failed',
					error.code || grpc.status.UNKNOWN,
					error.details
				));
			} else {
				resolve(response!);
			}
		});
	});
}

/**
 * Cashier service using gRPC
 */
export const cashierGrpcService = {
	async getCashiers(): Promise<Cashier[]> {
		const client = await initializeClient();
		const response = await promisifyCall<{ cashiers: Cashier[] }>(client, 'getCashiers', {});
		return response.cashiers || [];
	},

	async getCashier(cashierId: string): Promise<Cashier> {
		const client = await initializeClient();
		return await promisifyCall<Cashier>(client, 'getCashier', { cashierId });
	},

	async createCashier(cashierData: CreateCashierRequest): Promise<Cashier> {
		const client = await initializeClient();
		const request = {
			name: cashierData.name,
			email: cashierData.email,
			currencies: cashierData.currencies
		};
		return await promisifyCall<Cashier>(client, 'createCashier', request);
	},

	async updateCashier(cashierId: string, cashierData: UpdateCashierRequest): Promise<Cashier> {
		const client = await initializeClient();
		const request = {
			cashierId,
			...cashierData
		};
		return await promisifyCall<Cashier>(client, 'updateCashier', request);
	},

	async deleteCashier(cashierId: string): Promise<void> {
		const client = await initializeClient();
		await promisifyCall<{}>(client, 'deleteCashier', { cashierId });
	}
};

/**
 * Close gRPC client (for cleanup)
 */
export function closeGrpcClient(): void {
	if (client && typeof client.close === 'function') {
		client.close();
		client = null;
	}
}