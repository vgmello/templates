import { GrpcWebFetchTransport } from '@protobuf-ts/grpcweb-transport';
import { ResourceManagerClient } from './generated/resource_management.client';
import type { GetResourceRequest, GetResourceResponse } from './generated/resource_management';

// Determine the API URL. This might need to be configurable later,
// especially when running with Aspire. For now, a sensible default.
// Aspire will inject this via environment variables.
const apiUrl = process.env.VITE_API_URL || 'http://localhost:5271'; // Assuming Accounting.Api runs on this port by default

const transport = new GrpcWebFetchTransport({
    baseUrl: apiUrl
});

const client = new ResourceManagerClient(transport);

export async function getResource(resourceId: string): Promise<GetResourceResponse> {
    const request: GetResourceRequest = { resourceId };
    try {
        const call = await client.getResource(request, {});
        return call.response;
    } catch (error) {
        console.error('Error calling getResource:', error);
        throw error;
    }
}

// You might want to expose the client directly or create more specific functions
export default client;
