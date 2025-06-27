import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { env } from '$env/dynamic/private';
import { dev } from '$app/environment';
import type { 
	Cashier, CreateCashierRequest, UpdateCashierRequest,
	Invoice, CreateInvoiceRequest, MarkInvoiceAsPaidRequest, SimulatePaymentRequest
} from '../../app';

// Configuration
const GRPC_HOST = env.GRPC_HOST || 'localhost';
const GRPC_PORT = env.GRPC_PORT || '8102';
const CASHIER_PROTO_PATH = env.CASHIER_PROTO_PATH || './protos/cashiers.proto';
const INVOICE_PROTO_PATH = env.INVOICE_PROTO_PATH || './protos/invoices.proto';

// gRPC client interface
interface GrpcCallback<T = any> {
	(error: grpc.ServiceError | null, response?: T): void;
}

interface CashierGrpcClient {
	GetCashiers: (request: { limit?: number, offset?: number }, callback: GrpcCallback<{ cashiers: Cashier[] }>) => void;
	GetCashier: (request: { id: string }, callback: GrpcCallback<Cashier>) => void;
	CreateCashier: (request: { name: string, email: string }, callback: GrpcCallback<Cashier>) => void;
	UpdateCashier: (request: { cashierId: string, name?: string, email?: string }, callback: GrpcCallback<Cashier>) => void;
	DeleteCashier: (request: { cashierId: string }, callback: GrpcCallback<{}>) => void;
	close?: () => void;
}

interface InvoiceGrpcClient {
	GetInvoices: (request: { limit?: number, offset?: number, status?: string }, callback: GrpcCallback<{ invoices: Invoice[] }>) => void;
	GetInvoice: (request: { id: string }, callback: GrpcCallback<Invoice>) => void;
	CreateInvoice: (request: { name: string, amount: number, currency?: string, due_date?: any, cashierId?: string }, callback: GrpcCallback<Invoice>) => void;
	CancelInvoice: (request: { invoiceId: string }, callback: GrpcCallback<Invoice>) => void;
	MarkInvoiceAsPaid: (request: { invoiceId: string, amount_paid: number, payment_date?: any }, callback: GrpcCallback<Invoice>) => void;
	SimulatePayment: (request: { invoiceId: string, amount: number, currency?: string, payment_method?: string, payment_reference?: string }, callback: GrpcCallback<{ message: string }>) => void;
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

let cashierClient: CashierGrpcClient | null = null;
let invoiceClient: InvoiceGrpcClient | null = null;

/**
 * Initialize Cashier gRPC client
 */
async function initializeCashierClient(): Promise<CashierGrpcClient> {
	if (cashierClient) return cashierClient;

	try {
		// Load proto definition
		const packageDefinition = protoLoader.loadSync(CASHIER_PROTO_PATH, {
			keepCase: true,
			longs: String,
			enums: String,
			defaults: true,
			oneofs: true,
		});

		const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
		const CashiersService = protoDescriptor.billing.cashiers.CashiersService;

		// Create client
		cashierClient = new CashiersService(
			`${GRPC_HOST}:${GRPC_PORT}`,
			grpc.credentials.createInsecure()
		) as CashierGrpcClient;

		return cashierClient;
	} catch (error) {
		console.error('Failed to initialize Cashier gRPC client:', error);
		throw new GrpcError('Failed to initialize Cashier gRPC client', grpc.status.UNAVAILABLE, (error as Error).message);
	}
}

/**
 * Initialize Invoice gRPC client
 */
async function initializeInvoiceClient(): Promise<InvoiceGrpcClient> {
	if (invoiceClient) return invoiceClient;

	try {
		// Load proto definition
		const packageDefinition = protoLoader.loadSync(INVOICE_PROTO_PATH, {
			keepCase: true,
			longs: String,
			enums: String,
			defaults: true,
			oneofs: true,
		});

		const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
		const InvoicesService = protoDescriptor.billing.invoices.InvoicesService;

		// Create client
		invoiceClient = new InvoicesService(
			`${GRPC_HOST}:${GRPC_PORT}`,
			grpc.credentials.createInsecure()
		) as InvoiceGrpcClient;

		return invoiceClient;
	} catch (error) {
		console.error('Failed to initialize Invoice gRPC client:', error);
		throw new GrpcError('Failed to initialize Invoice gRPC client', grpc.status.UNAVAILABLE, (error as Error).message);
	}
}


/**
 * Promisify gRPC call
 */
function promisifyCall<T>(client: any, methodName: string, request: any): Promise<T> {
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
		const client = await initializeCashierClient();
		const response = await promisifyCall<{ cashiers: Cashier[] }>(client, 'GetCashiers', { limit: 100, offset: 0 });
		return response.cashiers || [];
	},

	async getCashier(cashierId: string): Promise<Cashier> {
		const client = await initializeCashierClient();
		return await promisifyCall<Cashier>(client, 'GetCashier', { id: cashierId });
	},

	async createCashier(cashierData: CreateCashierRequest): Promise<Cashier> {
		const client = await initializeCashierClient();
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
		const client = await initializeCashierClient();
		const request = {
			cashierId,
			name: cashierData.name,
			email: cashierData.email
		};
		return await promisifyCall<Cashier>(client, 'UpdateCashier', request);
	},

	async deleteCashier(cashierId: string): Promise<void> {
		const client = await initializeCashierClient();
		await promisifyCall<{}>(client, 'DeleteCashier', { cashierId });
	}
};

/**
 * Invoice service using gRPC
 */
export const invoiceGrpcService = {
	async getInvoices(limit = 100, offset = 0, status?: string): Promise<Invoice[]> {
		const client = await initializeInvoiceClient();
		const request: any = { limit, offset };
		if (status) request.status = status;
		
		const response = await promisifyCall<{ invoices: any[] }>(client, 'GetInvoices', request);
		return (response.invoices || []).map(transformGrpcInvoice);
	},

	async getInvoice(invoiceId: string): Promise<Invoice> {
		const client = await initializeInvoiceClient();
		const grpcInvoice = await promisifyCall<any>(client, 'GetInvoice', { id: invoiceId });
		return transformGrpcInvoice(grpcInvoice);
	},

	async createInvoice(invoiceData: CreateInvoiceRequest): Promise<Invoice> {
		const client = await initializeInvoiceClient();
		const request: any = {
			name: invoiceData.name,
			amount: invoiceData.amount,
			currency: invoiceData.currency || 'USD'
		};
		
		if (invoiceData.dueDate) {
			// Convert ISO string to protobuf Timestamp
			const date = new Date(invoiceData.dueDate);
			request.due_date = {
				seconds: Math.floor(date.getTime() / 1000),
				nanos: (date.getTime() % 1000) * 1000000
			};
		}
		
		if (invoiceData.cashierId) {
			request.cashierId = invoiceData.cashierId;
		}
		
		const grpcInvoice = await promisifyCall<any>(client, 'CreateInvoice', request);
		return transformGrpcInvoice(grpcInvoice);
	},

	async cancelInvoice(invoiceId: string): Promise<Invoice> {
		const client = await initializeInvoiceClient();
		const grpcInvoice = await promisifyCall<any>(client, 'CancelInvoice', { invoiceId });
		return transformGrpcInvoice(grpcInvoice);
	},

	async markInvoiceAsPaid(invoiceId: string, paymentData: MarkInvoiceAsPaidRequest): Promise<Invoice> {
		const client = await initializeInvoiceClient();
		const request: any = {
			invoiceId,
			amount_paid: paymentData.amountPaid
		};
		
		if (paymentData.paymentDate) {
			// Convert ISO string to protobuf Timestamp
			const date = new Date(paymentData.paymentDate);
			request.payment_date = {
				seconds: Math.floor(date.getTime() / 1000),
				nanos: (date.getTime() % 1000) * 1000000
			};
		}
		
		const grpcInvoice = await promisifyCall<any>(client, 'MarkInvoiceAsPaid', request);
		return transformGrpcInvoice(grpcInvoice);
	},

	async simulatePayment(invoiceId: string, paymentData: SimulatePaymentRequest): Promise<{ message: string }> {
		const client = await initializeInvoiceClient();
		const request = {
			invoiceId,
			amount: paymentData.amount,
			currency: paymentData.currency || 'USD',
			payment_method: paymentData.paymentMethod || 'Credit Card',
			payment_reference: paymentData.paymentReference || `SIM-${Date.now()}`
		};
		
		return await promisifyCall<{ message: string }>(client, 'SimulatePayment', request);
	}
};

/**
 * Transform gRPC Invoice to frontend Invoice
 */
function transformGrpcInvoice(grpcInvoice: any): Invoice {
	return {
		invoiceId: grpcInvoice.invoiceId || grpcInvoice.invoice_id,
		name: grpcInvoice.name,
		status: grpcInvoice.status,
		amount: grpcInvoice.amount,
		currency: grpcInvoice.currency || 'USD',
		dueDate: grpcInvoice.due_date || grpcInvoice.dueDate ? timestampToISOString(grpcInvoice.due_date || grpcInvoice.dueDate) : undefined,
		cashierId: grpcInvoice.cashierId || grpcInvoice.cashier_id || undefined,
		createdDateUtc: timestampToISOString(grpcInvoice.created_date_utc || grpcInvoice.createdDateUtc),
		updatedDateUtc: timestampToISOString(grpcInvoice.updated_date_utc || grpcInvoice.updatedDateUtc),
		version: grpcInvoice.version || 1
	};
}

/**
 * Convert protobuf Timestamp to ISO string
 */
function timestampToISOString(timestamp: any): string {
	if (!timestamp) return new Date().toISOString();
	
	if (typeof timestamp === 'string') return timestamp;
	
	if (timestamp.seconds !== undefined) {
		const date = new Date(timestamp.seconds * 1000 + (timestamp.nanos || 0) / 1000000);
		return date.toISOString();
	}
	
	return new Date().toISOString();
}

/**
 * Close gRPC clients (for cleanup)
 */
export function closeGrpcClient(): void {
	if (cashierClient && typeof cashierClient.close === 'function') {
		cashierClient.close();
		cashierClient = null;
	}
	if (invoiceClient && typeof invoiceClient.close === 'function') {
		invoiceClient.close();
		invoiceClient = null;
	}
}