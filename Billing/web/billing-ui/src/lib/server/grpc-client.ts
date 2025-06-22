import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { env } from '$env/dynamic/private';
import { dev } from '$app/environment';
import type { Cashier, CreateCashierRequest, UpdateCashierRequest } from '../../app';

// Configuration
const GRPC_HOST = env.GRPC_HOST || 'localhost';
const GRPC_PORT = env.GRPC_PORT || '4061';
const PROTO_PATH = env.PROTO_PATH || './protos/cashiers.proto';

// gRPC client interface
interface GrpcCallback<T = any> {
	(error: grpc.ServiceError | null, response?: T): void;
}

interface GrpcClient {
	GetCashiers: (request: { limit?: number, offset?: number }, callback: GrpcCallback<{ cashiers: Cashier[] }>) => void;
	GetCashier: (request: { id: string }, callback: GrpcCallback<Cashier>) => void;
	CreateCashier: (request: { name: string, email: string }, callback: GrpcCallback<Cashier>) => void;
	UpdateCashier: (request: { cashierId: string, name?: string, email?: string }, callback: GrpcCallback<Cashier>) => void;
	DeleteCashier: (request: { cashierId: string }, callback: GrpcCallback<{}>) => void;
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
		// Load proto definition
		const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
			keepCase: true,
			longs: String,
			enums: String,
			defaults: true,
			oneofs: true,
		});

		const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
		const CashiersService = protoDescriptor.billing.cashiers.CashiersService;

		// Create client
		client = new CashiersService(
			`${GRPC_HOST}:${GRPC_PORT}`,
			grpc.credentials.createInsecure()
		) as GrpcClient;

		return client;
	} catch (error) {
		console.error('Failed to initialize gRPC client:', error);
		throw new GrpcError('Failed to initialize gRPC client', grpc.status.UNAVAILABLE, (error as Error).message);
	}
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
		const response = await promisifyCall<{ cashiers: Cashier[] }>(client, 'GetCashiers', { limit: 100, offset: 0 });
		return response.cashiers || [];
	},

	async getCashier(cashierId: string): Promise<Cashier> {
		const client = await initializeClient();
		return await promisifyCall<Cashier>(client, 'GetCashier', { id: cashierId });
	},

	async createCashier(cashierData: CreateCashierRequest): Promise<Cashier> {
		const client = await initializeClient();
		const request = {
			name: cashierData.name,
			email: cashierData.email
			// Note: currencies are not supported by the current gRPC service
			// currencies: cashierData.currencies
		};
		const grpcCashier = await promisifyCall<any>(client, 'CreateCashier', request);
		
		// Transform the gRPC response to match our frontend expectations
		return {
			cashierId: grpcCashier.cashierId,
			name: grpcCashier.name,
			email: grpcCashier.email,
			cashierPayments: cashierData.currencies?.map(currency => ({
				currency,
				isActive: true,
				createdDateUtc: new Date().toISOString()
			})) || [],
			createdDateUtc: new Date().toISOString(),
			updatedDateUtc: new Date().toISOString(),
			version: 1
		};
	},

	async updateCashier(cashierId: string, cashierData: UpdateCashierRequest): Promise<Cashier> {
		const client = await initializeClient();
		const request = {
			cashierId,
			name: cashierData.name,
			email: cashierData.email
		};
		return await promisifyCall<Cashier>(client, 'UpdateCashier', request);
	},

	async deleteCashier(cashierId: string): Promise<void> {
		const client = await initializeClient();
		await promisifyCall<{}>(client, 'DeleteCashier', { cashierId });
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