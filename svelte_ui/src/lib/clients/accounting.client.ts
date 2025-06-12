import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import type {
  Ledger,
  GetLedgerRequest,
  GetLedgersRequest,
  GetLedgersResponse,
  CreateLedgerRequest,
  UpdateLedgerRequest,
  DeleteLedgerRequest,
  DeleteLedgerResponse
} from '../generated/accounting';

export class AccountingClient {
  private transport: GrpcWebFetchTransport;
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
    this.transport = new GrpcWebFetchTransport({
      baseUrl: baseUrl
    });
  }

  async getLedger(request: GetLedgerRequest): Promise<Ledger> {
    const response = await fetch(`${this.baseUrl}/api/ledgers/${request.ledger_id}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get ledger: ${response.statusText}`);
    }

    return response.json();
  }

  async getLedgers(request: GetLedgersRequest): Promise<GetLedgersResponse> {
    const params = new URLSearchParams({
      limit: request.limit.toString(),
      offset: request.offset.toString(),
    });

    if (request.client_id) {
      params.append('clientId', request.client_id);
    }

    const response = await fetch(`${this.baseUrl}/api/ledgers?${params}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get ledgers: ${response.statusText}`);
    }

    const ledgers = await response.json();
    return {
      ledgers: ledgers,
      total_count: ledgers.length // This should be implemented properly on the backend
    };
  }

  async createLedger(request: CreateLedgerRequest): Promise<Ledger> {
    const response = await fetch(`${this.baseUrl}/api/ledgers`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        clientId: request.client_id,
        ledgerType: request.ledger_type
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to create ledger: ${response.statusText}`);
    }

    return response.json();
  }

  async updateLedger(request: UpdateLedgerRequest): Promise<Ledger> {
    const response = await fetch(`${this.baseUrl}/api/ledgers/${request.ledger_id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        clientId: request.client_id,
        ledgerType: request.ledger_type
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update ledger: ${response.statusText}`);
    }

    return response.json();
  }

  async deleteLedger(request: DeleteLedgerRequest): Promise<DeleteLedgerResponse> {
    const response = await fetch(`${this.baseUrl}/api/ledgers/${request.ledger_id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to delete ledger: ${response.statusText}`);
    }

    return {
      success: true,
      message: 'Ledger deleted successfully'
    };
  }
}