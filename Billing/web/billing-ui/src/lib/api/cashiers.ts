import { credentials, loadPackageDefinition } from '@grpc/grpc-js';
import { loadSync } from '@grpc/proto-loader';
import path from 'path';

// Define TypeScript interfaces based on our proto files
export interface Cashier {
	tenantId: string;
	cashierId: string;
	name: string;
	email: string;
}

export interface GetCashierRequest {
	id: string;
}

export interface GetCashiersRequest {
	limit: number;
	offset: number;
}

export interface GetCashiersResponse {
	cashiers: Cashier[];
}

export interface CreateCashierRequest {
	name: string;
	email: string;
}

export interface UpdateCashierRequest {
	cashierId: string;
	name: string;
	email: string;
}

export interface DeleteCashierRequest {
	cashierId: string;
}

// gRPC service client type (will be properly typed when we load the service)
type CashiersServiceClient = any;

class CashierService {
	private client: CashiersServiceClient | null = null;

	private async getClient(): Promise<CashiersServiceClient> {
		if (this.client) {
			return this.client;
		}

		// Load the protobuf definition
		const protoPath = path.join(process.cwd(), 'src/lib/grpc/proto/Cashiers/cashiers.proto');
		const packageDefinition = loadSync(protoPath, {
			keepCase: true,
			longs: String,
			enums: String,
			defaults: true,
			oneofs: true,
			includeDirs: [path.join(process.cwd(), 'src/lib/grpc/proto')]
		});

		const billingProto = loadPackageDefinition(packageDefinition) as any;
		
		// Create the gRPC client
		const grpcUrl = 'localhost:8102'; // From the docker-compose setup
		this.client = new billingProto.billing.cashiers.CashiersService(
			grpcUrl,
			credentials.createInsecure()
		);

		return this.client;
	}

	async getCashier(id: string): Promise<Cashier> {
		const client = await this.getClient();
		return new Promise((resolve, reject) => {
			const request: GetCashierRequest = { id };
			client.GetCashier(request, (error: any, response: Cashier) => {
				if (error) {
					reject(error);
				} else {
					resolve(response);
				}
			});
		});
	}

	async getCashiers(limit: number = 50, offset: number = 0): Promise<GetCashiersResponse> {
		const client = await this.getClient();
		return new Promise((resolve, reject) => {
			const request: GetCashiersRequest = { limit, offset };
			client.GetCashiers(request, (error: any, response: GetCashiersResponse) => {
				if (error) {
					reject(error);
				} else {
					resolve(response);
				}
			});
		});
	}

	async createCashier(name: string, email: string): Promise<Cashier> {
		const client = await this.getClient();
		return new Promise((resolve, reject) => {
			const request: CreateCashierRequest = { name, email };
			client.CreateCashier(request, (error: any, response: Cashier) => {
				if (error) {
					reject(error);
				} else {
					resolve(response);
				}
			});
		});
	}

	async updateCashier(cashierId: string, name: string, email: string): Promise<Cashier> {
		const client = await this.getClient();
		return new Promise((resolve, reject) => {
			const request: UpdateCashierRequest = { cashierId, name, email };
			client.UpdateCashier(request, (error: any, response: Cashier) => {
				if (error) {
					reject(error);
				} else {
					resolve(response);
				}
			});
		});
	}

	async deleteCashier(cashierId: string): Promise<void> {
		const client = await this.getClient();
		return new Promise((resolve, reject) => {
			const request: DeleteCashierRequest = { cashierId };
			client.DeleteCashier(request, (error: any, response: any) => {
				if (error) {
					reject(error);
				} else {
					resolve();
				}
			});
		});
	}
}

// Singleton instance
export const cashierService = new CashierService();