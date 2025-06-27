# gRPC Integration

This guide covers gRPC integration in the Operations platform, including service registration, client configuration, and best practices.

## Overview

The Operations platform provides comprehensive gRPC support through the `Operations.ServiceDefaults.Api` package, enabling high-performance, strongly-typed communication between services.

## Service Registration

### Basic gRPC Service

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults (includes gRPC configuration)
builder.AddServiceDefaults();

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

// Map gRPC services
app.MapGrpcService<CashierService>();
app.MapGrpcService<InvoiceService>();

app.Run();
```

### Service Implementation

```csharp
public class CashierService : Cashier.CashierBase
{
    private readonly IMediator _mediator;

    public CashierService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreateCashierResponse> CreateCashier(
        CreateCashierRequest request, 
        ServerCallContext context)
    {
        var command = new CreateCashierCommand
        {
            Name = request.Name,
            Email = request.Email,
            Currencies = request.Currencies.ToList()
        };

        var result = await _mediator.Send(command, context.CancellationToken);

        return new CreateCashierResponse
        {
            Id = result.Id.ToString(),
            Name = result.Name,
            Email = result.Email,
            Currencies = { result.Currencies }
        };
    }
}
```

## Protocol Buffer Definitions

### Service Definition

```protobuf
syntax = "proto3";

package billing.v1;

service Cashier {
  rpc CreateCashier(CreateCashierRequest) returns (CreateCashierResponse);
  rpc GetCashier(GetCashierRequest) returns (GetCashierResponse);
  rpc UpdateCashier(UpdateCashierRequest) returns (UpdateCashierResponse);
  rpc DeleteCashier(DeleteCashierRequest) returns (DeleteCashierResponse);
  rpc ListCashiers(ListCashiersRequest) returns (ListCashiersResponse);
}

message CreateCashierRequest {
  string name = 1;
  string email = 2;
  repeated string currencies = 3;
}

message CreateCashierResponse {
  string id = 1;
  string name = 2;
  string email = 3;
  repeated string currencies = 4;
  google.protobuf.Timestamp created_at = 5;
}
```

### Message Types

```protobuf
message CashierInfo {
  string id = 1;
  string name = 2;
  string email = 3;
  repeated string currencies = 4;
  bool is_active = 5;
  google.protobuf.Timestamp created_at = 6;
  google.protobuf.Timestamp updated_at = 7;
}

message GetCashierRequest {
  string id = 1;
}

message GetCashierResponse {
  CashierInfo cashier = 1;
}

message ListCashiersRequest {
  int32 page_size = 1;
  string page_token = 2;
  string filter = 3;
}

message ListCashiersResponse {
  repeated CashierInfo cashiers = 1;
  string next_page_token = 2;
  int32 total_count = 3;
}
```

## Client Configuration

### gRPC Client Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register gRPC clients
builder.Services.AddGrpcClient<Cashier.CashierClient>(options =>
{
    options.Address = new Uri("https://billing-api:8102");
});

builder.Services.AddGrpcClient<Invoice.InvoiceClient>(options =>
{
    options.Address = new Uri("https://billing-api:8102");
});

var app = builder.Build();
```

### Client Usage

```csharp
public class BillingService
{
    private readonly Cashier.CashierClient _cashierClient;
    private readonly Invoice.InvoiceClient _invoiceClient;

    public BillingService(
        Cashier.CashierClient cashierClient,
        Invoice.InvoiceClient invoiceClient)
    {
        _cashierClient = cashierClient;
        _invoiceClient = invoiceClient;
    }

    public async Task<CashierInfo> GetCashierAsync(Guid cashierId)
    {
        var request = new GetCashierRequest { Id = cashierId.ToString() };
        var response = await _cashierClient.GetCashierAsync(request);
        return response.Cashier;
    }

    public async Task<CreateCashierResponse> CreateCashierAsync(
        string name, 
        string email, 
        IEnumerable<string> currencies)
    {
        var request = new CreateCashierRequest
        {
            Name = name,
            Email = email,
            Currencies = { currencies }
        };

        return await _cashierClient.CreateCashierAsync(request);
    }
}
```

## Advanced Features

### Streaming Services

```csharp
public class PaymentService : Payment.PaymentBase
{
    public override async Task ProcessPayments(
        IAsyncStreamReader<ProcessPaymentRequest> requestStream,
        IServerStreamWriter<ProcessPaymentResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            var response = await ProcessSinglePayment(request);
            await responseStream.WriteAsync(response);
        }
    }
}
```

### Metadata and Headers

```csharp
public class AuthenticatedCashierService : Cashier.CashierBase
{
    public override async Task<CreateCashierResponse> CreateCashier(
        CreateCashierRequest request, 
        ServerCallContext context)
    {
        // Extract metadata
        var correlationId = context.RequestHeaders
            .GetValue("x-correlation-id");
        
        var userId = context.RequestHeaders
            .GetValue("x-user-id");

        // Add response metadata
        context.ResponseTrailers.Add("x-response-time", 
            DateTime.UtcNow.ToString("O"));

        // Implementation...
    }
}
```

### Error Handling

```csharp
public class CashierService : Cashier.CashierBase
{
    public override async Task<GetCashierResponse> GetCashier(
        GetCashierRequest request, 
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var cashierId))
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument, 
                    "Invalid cashier ID format"));
            }

            var cashier = await _repository.GetCashierAsync(cashierId);
            if (cashier == null)
            {
                throw new RpcException(new Status(
                    StatusCode.NotFound, 
                    $"Cashier with ID {cashierId} not found"));
            }

            return new GetCashierResponse { Cashier = MapToCashierInfo(cashier) };
        }
        catch (Exception ex) when (!(ex is RpcException))
        {
            throw new RpcException(new Status(
                StatusCode.Internal, 
                "An error occurred while retrieving the cashier"));
        }
    }
}
```

## Service Discovery Integration

### Aspire Integration

```csharp
// In AppHost project
var billingApi = builder.AddProject<Projects.Billing_Api>("billing-api")
    .WithHttpEndpoint(port: 8101, name: "http")
    .WithHttpsEndpoint(port: 8111, name: "https")
    .WithHttpEndpoint(port: 8102, name: "grpc");

var accountingApi = builder.AddProject<Projects.Accounting_Api>("accounting-api")
    .WithReference(billingApi);
```

### Client Configuration with Service Discovery

```csharp
// Automatic service discovery
builder.Services.AddGrpcClient<Cashier.CashierClient>(options =>
{
    options.Address = new Uri("https://billing-api");
});

// Manual configuration
builder.Services.AddGrpcClient<Cashier.CashierClient>(options =>
{
    options.Address = new Uri(
        builder.Configuration.GetConnectionString("BillingGrpc") ?? 
        "https://localhost:8102");
});
```

## Security and Authentication

### TLS Configuration

```csharp
// Server-side TLS
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = LoadCertificate();
    });
});
```

### Authentication

```csharp
public class AuthenticatedCashierService : Cashier.CashierBase
{
    public override async Task<CreateCashierResponse> CreateCashier(
        CreateCashierRequest request, 
        ServerCallContext context)
    {
        // Check authentication
        var user = context.GetHttpContext().User;
        if (!user.Identity?.IsAuthenticated == true)
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated, 
                "Authentication required"));
        }

        // Check authorization
        if (!user.HasClaim("permission", "cashier:create"))
        {
            throw new RpcException(new Status(
                StatusCode.PermissionDenied, 
                "Insufficient permissions"));
        }

        // Implementation...
    }
}
```

## Performance and Monitoring

### Compression

```csharp
builder.Services.AddGrpc(options =>
{
    options.ResponseCompressionLevel = CompressionLevel.Optimal;
    options.ResponseCompressionAlgorithm = "gzip";
});
```

### Metrics and Telemetry

```csharp
// Automatic OpenTelemetry integration
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddGrpcClientInstrumentation()
        .AddGrpcCoreInstrumentation());
```

### Health Checks

```csharp
builder.Services.AddGrpcHealthChecks()
    .AddCheck("cashier-service", () => HealthCheckResult.Healthy());

// In service
app.MapGrpcHealthChecksService();
```

## Best Practices

1. **Service Contracts**: Define clear, versioned service contracts
2. **Error Handling**: Use appropriate gRPC status codes
3. **Streaming**: Use streaming for large datasets or real-time updates
4. **Metadata**: Include correlation IDs and request context
5. **Performance**: Enable compression and connection pooling
6. **Security**: Always use TLS in production
7. **Monitoring**: Implement comprehensive observability

## Code Generation

### Project Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <Protobuf Include="Protos\cashier.proto" GrpcServices="Server" />
    <Protobuf Include="Protos\invoice.proto" GrpcServices="Server" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="Grpc.AspNetCore" />
    <PackageReference Include="Google.Protobuf" />
    <PackageReference Include="Grpc.Tools" PrivateAssets="All" />
  </ItemGroup>
</Project>
```

### Generated Code Usage

```csharp
// Generated client
var client = new Cashier.CashierClient(channel);

// Generated service base
public class CashierService : Cashier.CashierBase
{
    // Implementation
}

// Generated messages
var request = new CreateCashierRequest
{
    Name = "John Doe",
    Email = "john@example.com",
    Currencies = { "USD", "EUR" }
};
```

## See Also

- [API Overview](overview.md)
- [OpenAPI Documentation](openapi/overview.md)
- [Service Defaults](../architecture/service-defaults.md)