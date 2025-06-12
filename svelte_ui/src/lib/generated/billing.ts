// Generated from billing.proto

export interface Cashier {
  cashier_id: string;
  name: string;
  email: string;
  created_at: string;
  updated_at: string;
  currencies: CashierCurrency[];
  is_active: boolean;
}

export interface CashierCurrency {
  currency: string;
  balance: number;
  last_updated: string;
}

export interface Invoice {
  invoice_id: string;
  cashier_id: string;
  amount: number;
  currency: string;
  description: string;
  status: InvoiceStatus;
  created_at: string;
  updated_at: string;
  due_date: string;
}

export enum InvoiceStatus {
  INVOICE_STATUS_UNSPECIFIED = 0,
  INVOICE_STATUS_DRAFT = 1,
  INVOICE_STATUS_PENDING = 2,
  INVOICE_STATUS_PAID = 3,
  INVOICE_STATUS_CANCELLED = 4,
  INVOICE_STATUS_OVERDUE = 5,
}

// Cashier interfaces
export interface GetCashierRequest {
  cashier_id: string;
}

export interface GetCashiersRequest {
  limit: number;
  offset: number;
  active_only: boolean;
}

export interface GetCashiersResponse {
  cashiers: Cashier[];
  total_count: number;
}

export interface CreateCashierRequest {
  name: string;
  email: string;
}

export interface UpdateCashierRequest {
  cashier_id: string;
  name: string;
  email: string;
  is_active: boolean;
}

export interface DeleteCashierRequest {
  cashier_id: string;
}

export interface DeleteCashierResponse {
  success: boolean;
  message: string;
}

// Invoice interfaces
export interface GetInvoiceRequest {
  invoice_id: string;
}

export interface GetInvoicesRequest {
  limit: number;
  offset: number;
  cashier_id?: string;
  status?: InvoiceStatus;
}

export interface GetInvoicesResponse {
  invoices: Invoice[];
  total_count: number;
}

export interface CreateInvoiceRequest {
  cashier_id: string;
  amount: number;
  currency: string;
  description: string;
  due_date: string;
}

export interface UpdateInvoiceRequest {
  invoice_id: string;
  amount: number;
  description: string;
  status: InvoiceStatus;
  due_date: string;
}

export interface DeleteInvoiceRequest {
  invoice_id: string;
}

export interface DeleteInvoiceResponse {
  success: boolean;
  message: string;
}