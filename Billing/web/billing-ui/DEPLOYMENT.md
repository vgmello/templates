# Edge Deployment Guide

This SvelteKit application has been optimized for edge deployment on CDN platforms like Vercel, Netlify, or Cloudflare.

## Key Features for Edge Deployment

### Server-Side Architecture
- **gRPC Client**: All data fetching moved to server-side using gRPC
- **SSR Data Loading**: Pages use `+page.server.js` for server-side data fetching
- **Form Actions**: Mutations (create, update, delete) use SvelteKit form actions
- **No Client-Side API Calls**: Eliminates browser HTTP requests to backend

### Edge-Compatible Design
- **Stateless Operations**: Each request is independent
- **Graceful Degradation**: Handles service unavailability
- **Fast Cold Starts**: Minimal server-side dependencies
- **Mock Fallbacks**: Development mode with mock data

## Environment Variables

Configure these environment variables for your deployment:

```bash
# Required for production
GRPC_HOST=your-grpc-host.com
GRPC_PORT=443
PROTO_PATH=/path/to/billing.proto

# Optional
NODE_ENV=production
```

## Deployment Platforms

### Vercel
```json
{
  "build": {
    "env": {
      "GRPC_HOST": "api.your-domain.com",
      "GRPC_PORT": "443"
    }
  }
}
```

### Netlify
```toml
[build.environment]
  GRPC_HOST = "api.your-domain.com"
  GRPC_PORT = "443"
```

### Cloudflare Pages
Set environment variables in the Cloudflare dashboard or use `wrangler.toml`:

```toml
[env.production]
GRPC_HOST = "api.your-domain.com"
GRPC_PORT = "443"
```

## Performance Benefits

1. **Reduced Latency**: Server-side gRPC calls are faster than browser HTTP
2. **Better Caching**: Static pages can be cached at edge locations
3. **Improved Security**: API credentials stay on server
4. **Lower Client Bundle**: No API client code shipped to browser

## Monitoring & Debugging

The application includes:
- Error handling for gRPC connection failures
- Service unavailability indicators
- Development mode with mock data
- Comprehensive logging for debugging

## Testing Edge Compatibility

Run locally to test edge-like behavior:
```bash
npm run build
npm run preview
```

The build should complete without errors and the preview should work with mock data when gRPC services are unavailable.