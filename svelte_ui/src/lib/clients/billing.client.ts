import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import type {
  Cashier,
  Invoice,
  GetCashierRequest,
  GetCashiersRequest,
  GetCashiersResponse,
  CreateCashierRequest,
  UpdateCashierRequest,
  DeleteCashierRequest,
  DeleteCashierResponse,
  GetInvoiceRequest,
  GetInvoicesRequest,
  GetInvoicesResponse,
  CreateInvoiceRequest,
  UpdateInvoiceRequest,
  DeleteInvoiceRequest,
  DeleteInvoiceResponse
} from '../generated/billing';

export class BillingClient {
  private transport: GrpcWebFetchTransport;
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
    this.transport = new GrpcWebFetchTransport({
      baseUrl: baseUrl
    });
  }

  // Cashier methods
  async getCashier(request: GetCashierRequest): Promise<Cashier> {
    const response = await fetch(`${this.baseUrl}/api/cashiers/${request.cashier_id}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get cashier: ${response.statusText}`);
    }

    return response.json();
  }

  async getCashiers(request: GetCashiersRequest): Promise<GetCashiersResponse> {
    const params = new URLSearchParams({
      limit: request.limit.toString(),
      offset: request.offset.toString(),
    });

    if (request.active_only) {
      params.append('activeOnly', 'true');
    }

    const response = await fetch(`${this.baseUrl}/api/cashiers?${params}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get cashiers: ${response.statusText}`);
    }

    const cashiers = await response.json();
    return {
      cashiers: cashiers,
      total_count: cashiers.length // This should be implemented properly on the backend
    };
  }

  async createCashier(request: CreateCashierRequest): Promise<Cashier> {
    const response = await fetch(`${this.baseUrl}/api/cashiers`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: request.name,
        email: request.email
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to create cashier: ${response.statusText}`);
    }

    return response.json();
  }

  async updateCashier(request: UpdateCashierRequest): Promise<Cashier> {
    const response = await fetch(`${this.baseUrl}/api/cashiers/${request.cashier_id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: request.name,
        email: request.email,
        isActive: request.is_active
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update cashier: ${response.statusText}`);
    }

    return response.json();
  }

  async deleteCashier(request: DeleteCashierRequest): Promise<DeleteCashierResponse> {
    const response = await fetch(`${this.baseUrl}/api/cashiers/${request.cashier_id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to delete cashier: ${response.statusText}`);
    }

    return {
      success: true,
      message: 'Cashier deleted successfully'
    };
  }

  // Invoice methods
  async getInvoice(request: GetInvoiceRequest): Promise<Invoice> {
    const response = await fetch(`${this.baseUrl}/api/invoices/${request.invoice_id}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get invoice: ${response.statusText}`);
    }

    return response.json();
  }

  async getInvoices(request: GetInvoicesRequest): Promise<GetInvoicesResponse> {
    const params = new URLSearchParams({
      limit: request.limit.toString(),
      offset: request.offset.toString(),
    });

    if (request.cashier_id) {
      params.append('cashierId', request.cashier_id);
    }

    if (request.status !== undefined) {
      params.append('status', request.status.toString());
    }

    const response = await fetch(`${this.baseUrl}/api/invoices?${params}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get invoices: ${response.statusText}`);
    }

    const invoices = await response.json();
    return {
      invoices: invoices,
      total_count: invoices.length // This should be implemented properly on the backend
    };
  }

  async createInvoice(request: CreateInvoiceRequest): Promise<Invoice> {
    const response = await fetch(`${this.baseUrl}/api/invoices`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        cashierId: request.cashier_id,
        amount: request.amount,
        currency: request.currency,
        description: request.description,
        dueDate: request.due_date
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to create invoice: ${response.statusText}`);
    }

    return response.json();
  }

  async updateInvoice(request: UpdateInvoiceRequest): Promise<Invoice> {
    const response = await fetch(`${this.baseUrl}/api/invoices/${request.invoice_id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        amount: request.amount,
        description: request.description,
        status: request.status,
        dueDate: request.due_date
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update invoice: ${response.statusText}`);
    }

    return response.json();
  }

  async deleteInvoice(request: DeleteInvoiceRequest): Promise<DeleteInvoiceResponse> {
    const response = await fetch(`${this.baseUrl}/api/invoices/${request.invoice_id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to delete invoice: ${response.statusText}`);
    }

    return {
      success: true,
      message: 'Invoice deleted successfully'
    };
  }
}