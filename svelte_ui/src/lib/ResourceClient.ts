import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import type { 
  GetResourceRequest, 
  GetResourceResponse,
  GetDashboardStatsRequest,
  GetDashboardStatsResponse,
  GetOverviewRequest,
  GetOverviewResponse,
  GetSystemHealthRequest,
  GetSystemHealthResponse
} from './generated/resource_management';

// API URLs from environment variables
const accountingApiUrl = import.meta.env.VITE_ACCOUNTING_API_URL || 'http://localhost:5271';
const billingApiUrl = import.meta.env.VITE_BILLING_API_URL || 'http://localhost:5272';
const platformApiUrl = import.meta.env.VITE_PLATFORM_API_URL || 'http://localhost:5270';

export class ResourceClient {
  private transport: GrpcWebFetchTransport;
  private baseUrl: string;

  constructor(baseUrl: string = platformApiUrl) {
    this.baseUrl = baseUrl;
    this.transport = new GrpcWebFetchTransport({
      baseUrl: baseUrl
    });
  }

  async getResource(resourceId: string): Promise<GetResourceResponse> {
    const response = await fetch(`${this.baseUrl}/api/resources/${resourceId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get resource: ${response.statusText}`);
    }

    return response.json();
  }

  async getDashboardStats(request: GetDashboardStatsRequest): Promise<GetDashboardStatsResponse> {
    const params = new URLSearchParams();
    if (request.time_range) {
      params.append('timeRange', request.time_range);
    }

    const response = await fetch(`${this.baseUrl}/api/dashboard/stats?${params}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get dashboard stats: ${response.statusText}`);
    }

    return response.json();
  }

  async getOverview(request: GetOverviewRequest = {}): Promise<GetOverviewResponse> {
    const params = new URLSearchParams();
    if (request.user_id) {
      params.append('userId', request.user_id);
    }

    const response = await fetch(`${this.baseUrl}/api/dashboard/overview?${params}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get overview: ${response.statusText}`);
    }

    return response.json();
  }

  async getSystemHealth(request: GetSystemHealthRequest = { include_details: false }): Promise<GetSystemHealthResponse> {
    const params = new URLSearchParams();
    if (request.include_details) {
      params.append('includeDetails', 'true');
    }

    const response = await fetch(`${this.baseUrl}/api/health?${params}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get system health: ${response.statusText}`);
    }

    return response.json();
  }
}

// Create default client instance
const defaultClient = new ResourceClient();

// Export convenience functions
export async function getResource(resourceId: string): Promise<GetResourceResponse> {
  return defaultClient.getResource(resourceId);
}

export async function getDashboardStats(timeRange: string = '24h'): Promise<GetDashboardStatsResponse> {
  return defaultClient.getDashboardStats({ time_range: timeRange });
}

export async function getOverview(): Promise<GetOverviewResponse> {
  return defaultClient.getOverview();
}

export async function getSystemHealth(includeDetails: boolean = false): Promise<GetSystemHealthResponse> {
  return defaultClient.getSystemHealth({ include_details: includeDetails });
}

export default defaultClient;
