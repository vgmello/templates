// Generated from accounting.proto

export interface Ledger {
  ledger_id: string;
  client_id: string;
  ledger_type: number;
  created_at: string;
  updated_at: string;
  balances: LedgerBalance[];
}

export interface LedgerBalance {
  currency: string;
  amount: number;
  last_updated: string;
}

export interface GetLedgerRequest {
  ledger_id: string;
}

export interface GetLedgersRequest {
  limit: number;
  offset: number;
  client_id?: string;
}

export interface GetLedgersResponse {
  ledgers: Ledger[];
  total_count: number;
}

export interface CreateLedgerRequest {
  client_id: string;
  ledger_type: number;
}

export interface UpdateLedgerRequest {
  ledger_id: string;
  client_id: string;
  ledger_type: number;
}

export interface DeleteLedgerRequest {
  ledger_id: string;
}

export interface DeleteLedgerResponse {
  success: boolean;
  message: string;
}