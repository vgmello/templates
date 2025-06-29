import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { env } from '$env/dynamic/private';
import { dev } from '$app/environment';
import { trace, context, propagation } from '@opentelemetry/api';
import type { 
	Cashier, CreateCashierRequest, UpdateCashierRequest,
	Invoice, CreateInvoiceRequest, MarkInvoiceAsPaidRequest, SimulatePaymentRequest
} from '../../app';

// Mock mode for testing
const MOCK_API = env.MOCK_API === 'true' || process.env.MOCK_API === 'true' || (typeof process !== 'undefined' && process.env?.MOCK_API === 'true');

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

// Create a tracer for gRPC client operations
const tracer = trace.getTracer('billing-ui-grpc-client', '1.0.0');

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
 * Promisify gRPC call with OpenTelemetry tracing
 */
function promisifyCall<T>(client: any, methodName: string, request: any): Promise<T> {
	return new Promise((resolve, reject) => {
		const span = tracer.startSpan(`grpc.${methodName}`, {
			kind: 1, // CLIENT span kind
			attributes: {
				'rpc.system': 'grpc',
				'rpc.service': client.constructor.name,
				'rpc.method': methodName,
				'rpc.grpc.status_code': grpc.status.OK,
			}
		});

		// Inject trace context into gRPC metadata
		const metadata = new grpc.Metadata();
		const carrier: Record<string, string> = {};
		
		// Propagate trace context
		propagation.inject(trace.setSpan(context.active(), span), carrier);
		
		// Add trace headers to gRPC metadata
		Object.entries(carrier).forEach(([key, value]) => {
			metadata.add(key, value);
		});

		// Make the gRPC call with metadata
		const call = (client[methodName] as any)(request, metadata, (error: grpc.ServiceError | null, response?: T) => {
			if (error) {
				span.recordException(error);
				span.setStatus({
					code: 2, // ERROR
					message: error.message || 'gRPC call failed',
				});
				span.setAttributes({
					'rpc.grpc.status_code': error.code || grpc.status.UNKNOWN,
				});
				span.end();
				
				reject(new GrpcError(
					error.message || 'gRPC call failed',
					error.code || grpc.status.UNKNOWN,
					error.details
				));
			} else {
				span.setStatus({ code: 1 }); // OK
				span.end();
				resolve(response!);
			}
		});

		// Handle call cancellation
		call.on('error', (error: Error) => {
			span.recordException(error);
			span.setStatus({
				code: 2, // ERROR
				message: error.message,
			});
			span.end();
		});
	});
}

/**
 * Cashier service using gRPC
 */
export const cashierGrpcService = {
	async getCashiers(): Promise<Cashier[]> {
		if (MOCK_API) {
			return getMockCashiers();
		}

		const client = await initializeCashierClient();
		const response = await promisifyCall<{ cashiers: Cashier[] }>(client, 'GetCashiers', { limit: 100, offset: 0 });
		return response.cashiers || [];
	},

	async getCashier(cashierId: string): Promise<Cashier> {
		if (MOCK_API) {
			const mockCashiers = getMockCashiers();
			const cashier = mockCashiers.find(c => c.cashierId === cashierId);
			if (!cashier) {
				throw new GrpcError('Cashier not found', grpc.status.NOT_FOUND);
			}
			return cashier;
		}

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
 * Mock data for testing
 */
function getMockInvoices(): Invoice[] {
	return [
		{
			invoiceId: "mock-invoice-1",
			name: "Mock Invoice 1",
			status: "Draft",
			amount: 100.00,
			currency: "USD",
			dueDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
			cashierId: "mock-cashier-1",
			createdDateUtc: new Date().toISOString(),
			updatedDateUtc: new Date().toISOString(),
			version: 1
		},
		{
			invoiceId: "mock-invoice-2",
			name: "Mock Invoice 2",
			status: "Paid",
			amount: 250.50,
			currency: "USD",
			dueDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
			createdDateUtc: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString(),
			updatedDateUtc: new Date().toISOString(),
			version: 2
		}
	];
}

function getMockCashiers(): Cashier[] {
	return [
		{
			cashierId: "mock-cashier-1", 
			name: "Mock Cashier",
			email: "mock@example.com",
			cashierPayments: [
				{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() }
			],
			createdDateUtc: new Date().toISOString(),
			updatedDateUtc: new Date().toISOString(),
			version: 1
		}
	];
}

/**
 * Invoice service using gRPC
 */
export const invoiceGrpcService = {
	async getInvoices(limit = 100, offset = 0, status?: string): Promise<Invoice[]> {
		if (MOCK_API) {
			const mockInvoices = getMockInvoices();
			return status ? mockInvoices.filter(inv => inv.status === status) : mockInvoices;
		}

		const client = await initializeInvoiceClient();
		const request: any = { limit, offset };
		if (status) request.status = status;
		
		const response = await promisifyCall<{ invoices: any[] }>(client, 'GetInvoices', request);
		return (response.invoices || []).map(transformGrpcInvoice);
	},

	async getInvoice(invoiceId: string): Promise<Invoice> {
		if (MOCK_API) {
			const mockInvoices = getMockInvoices();
			const invoice = mockInvoices.find(inv => inv.invoiceId === invoiceId);
			if (!invoice) {
				throw new GrpcError('Invoice not found', grpc.status.NOT_FOUND);
			}
			return invoice;
		}

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