// Generated from resource_management.proto

export interface GetResourceRequest {
  resource_id: string;
}

export interface GetResourceResponse {
  resource_id: string;
  name: string;
  description: string;
  resource_type: string;
  status: string;
  created_at: string;
  updated_at: string;
}

export interface GetDashboardStatsRequest {
  time_range: string;
}

export interface GetDashboardStatsResponse {
  total_ledgers: number;
  total_cashiers: number;
  total_invoices: number;
  pending_invoices: number;
  total_revenue: number;
  currency: string;
  service_statuses: ServiceStatus[];
}

export interface ServiceStatus {
  service_name: string;
  status: string;
  last_check: string;
  version: string;
}

export interface GetSystemHealthRequest {
  include_details: boolean;
}

export interface GetSystemHealthResponse {
  overall_status: string;
  health_checks: HealthCheck[];
  timestamp: string;
}

export interface HealthCheck {
  component: string;
  status: string;
  message: string;
  details: Record<string, string>;
}

export interface GetOverviewRequest {
  user_id?: string;
}

export interface GetOverviewResponse {
  stats: DashboardStats;
  recent_activities: RecentActivity[];
  alerts: SystemAlert[];
}

export interface DashboardStats {
  active_ledgers: number;
  active_cashiers: number;
  pending_invoices: number;
  overdue_invoices: number;
  total_outstanding: number;
  total_paid_today: number;
  currency: string;
}

export interface RecentActivity {
  activity_id: string;
  activity_type: string;
  description: string;
  timestamp: string;
  user_id: string;
  metadata: Record<string, string>;
}

export interface SystemAlert {
  alert_id: string;
  severity: string;
  title: string;
  message: string;
  timestamp: string;
  acknowledged: boolean;
}

export interface GetRecentActivityRequest {
  limit: number;
  activity_type?: string;
  since?: string;
}

export interface GetRecentActivityResponse {
  activities: RecentActivity[];
  next_cursor: string;
}