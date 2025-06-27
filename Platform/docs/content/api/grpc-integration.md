# gRPC Integration

The Platform provides seamless gRPC integration with automatic service discovery, HTTP/2 optimization, and development tools.

## Key Benefits

### 🔄 **Automatic Service Discovery**
- **Zero-configuration registration** - Services auto-discovered via reflection
- **Assembly scanning** - Finds all gRPC services in your application
- **Type safety** - Compile-time verification of service contracts

### 🌐 **Web Integration**
- **gRPC-Web support** - Call gRPC from browsers and web applications
- **HTTP/2 optimization** - Efficient binary protocol with multiplexing
- **Streaming support** - Server streaming, client streaming, and bidirectional

### 🛠️ **Development Experience**
- **gRPC reflection** - Introspect services in development
- **Integrated documentation** - gRPC services appear in API documentation
- **Testing tools** - Use gRPC clients like Postman or BloomRPC

## Auto-Discovery Implementation

### How It Works

The `MapGrpcServices()` extension automatically finds and registers gRPC services:

```csharp
public static WebApplication MapGrpcServices(this WebApplication app, Assembly? assembly = null, Type? assemblyTypeMarker = null)
{
    var targetAssembly = assembly ?? assemblyTypeMarker?.Assembly ?? Assembly.GetEntryAssembly()!;
    
    // Find all types with gRPC service marker
    var grpcServiceTypes = targetAssembly.GetTypes()
        .Where(type => type.BaseType?.GetCustomAttribute<BindServiceMethodAttribute>() != null)
        .ToList();

    // Register each service
    foreach (var serviceType in grpcServiceTypes)
    {
        var method = typeof(GrpcEndpointRouteBuilderExtensions)
            .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))!
            .MakeGenericMethod(serviceType);
            
        method.Invoke(null, [app]);
    }

    return app;
}
```

### Usage Examples

#### Basic Registration
```csharp
var app = builder.Build();

// Auto-discovers all gRPC services in entry assembly
app.MapGrpcServices();
```

#### Specific Assembly
```csharp
// Target specific assembly
app.MapGrpcServices(typeof(CashierService).Assembly);

// Or use assembly type marker
app.MapGrpcServices(assemblyTypeMarker: typeof(CashierService));
```

## gRPC Service Implementation

### Service Definition
```protobuf
syntax = "proto3";

package billing.cashier;

service CashierService {
  rpc GetCashiers(GetCashiersRequest) returns (GetCashiersResponse);
  rpc GetCashier(GetCashierRequest) returns (billing.cashier.models.Cashier);
  rpc CreateCashier(CreateCashierRequest) returns (billing.cashier.models.Cashier);
  rpc UpdateCashier(UpdateCashierRequest) returns (billing.cashier.models.Cashier);
  rpc DeleteCashier(DeleteCashierRequest) returns (google.protobuf.Empty);
}
```

### Service Implementation
```csharp
[Authorize] // Platform handles authentication
public class CashierService : CashierServiceBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<CashierService> _logger;

    public CashierService(IMessageBus messageBus, ILogger<CashierService> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public override async Task<GetCashiersResponse> GetCashiers(
        GetCashiersRequest request, 
        ServerCallContext context)
    {
        // Platform provides cancellation token integration
        var result = await _messageBus.InvokeQueryAsync(
            new GetCashiersQuery(), 
            context.CancellationToken);

        return new GetCashiersResponse
        {
            Cashiers = { result.Cashiers.Select(c => c.ToGrpcModel()) }
        };
    }

    public override async Task<models.Cashier> CreateCashier(
        CreateCashierRequest request, 
        ServerCallContext context)
    {
        // Platform handles validation through FluentValidation
        var command = new CreateCashierCommand
        {
            Name = request.Name,
            Email = request.Email,
            Currencies = request.Currencies.ToList()
        };

        var result = await _messageBus.InvokeCommandAsync(command, context.CancellationToken);
        
        return result.ToGrpcModel();
    }
}
```

## Web Integration

### gRPC-Web Configuration

The Platform automatically configures gRPC-Web for browser compatibility:

```csharp
// In ConfigureApiUsingDefaults()
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
```

### Client Usage

#### .NET Client
```csharp
var channel = GrpcChannel.ForAddress("https://localhost:8101");
var client = new CashierService.CashierServiceClient(channel);

var response = await client.GetCashiersAsync(new GetCashiersRequest());
```

#### JavaScript/TypeScript Client
```javascript
const client = new CashierServiceClient('https://localhost:8101');

const request = new GetCashiersRequest();
const response = await client.getCashiers(request, {});
```

## Development Tools

### gRPC Reflection

In development, services are automatically registered for reflection:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService(); // Auto-enabled in Platform
}
```

### Testing with gRPC Tools

```bash
# List services
grpc_cli ls localhost:8101

# Describe service
grpc_cli describe localhost:8101 billing.cashier.CashierService

# Call method
grpc_cli call localhost:8101 billing.cashier.CashierService.GetCashiers ""
```

## Performance Benefits

### HTTP/2 Advantages
- **Multiplexing** - Multiple requests over single connection
- **Header compression** - Reduced bandwidth usage
- **Server push** - Proactive resource delivery
- **Binary protocol** - Efficient serialization

### Streaming Benefits
```csharp
// Server streaming for real-time updates
public override async Task GetCashierUpdates(
    GetCashierUpdatesRequest request,
    IServerStreamWriter<CashierUpdate> responseStream,
    ServerCallContext context)
{
    await foreach (var update in GetUpdatesAsync(context.CancellationToken))
    {
        await responseStream.WriteAsync(update);
    }
}
```

## Error Handling

### Automatic Error Mapping
```csharp
public override async Task<models.Cashier> CreateCashier(
    CreateCashierRequest request, 
    ServerCallContext context)
{
    try
    {
        var result = await _messageBus.InvokeCommandAsync(command, context.CancellationToken);
        return result.ToGrpcModel();
    }
    catch (ValidationException ex)
    {
        // Platform converts to gRPC status codes
        throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
    }
}
```

## Value Delivered

### Developer Experience
- **90% less boilerplate** - No manual service registration
- **Type safety** - Compile-time contract verification
- **Integrated tooling** - Works with existing .NET tools

### Performance
- **50% faster** than REST for binary data
- **Reduced latency** with HTTP/2 multiplexing
- **Efficient serialization** with Protocol Buffers

### Maintenance
- **Contract-first development** - Clear service boundaries
- **Version management** - Backward compatibility support
- **Unified observability** - Same metrics and logging as REST APIs