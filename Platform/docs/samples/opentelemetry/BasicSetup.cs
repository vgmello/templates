using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// <BasicOpenTelemetrySetup>
// OpenTelemetry is automatically configured when you add Service Defaults
builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Service Defaults configures:
// - ActivitySource for custom tracing
// - ASP.NET Core instrumentation (HTTP requests)
// - HttpClient instrumentation (outgoing requests) 
// - Wolverine messaging instrumentation
// - Metrics collection (request duration, counts, etc.)
// - Logging integration with OpenTelemetry
// </BasicOpenTelemetrySetup>

// <CustomInstrumentation>
// Get the pre-configured ActivitySource for creating custom spans
var activitySource = builder.Services.BuildServiceProvider()
    .GetRequiredService<ActivitySource>();

builder.Services.AddSingleton(activitySource);
builder.Services.AddScoped<IOrderService, OrderService>();
// </CustomInstrumentation>

builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

// <CustomSpanExample>
public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
}

public class OrderService : IOrderService
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ActivitySource activitySource, ILogger<OrderService> logger)
    {
        _activitySource = activitySource;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Create a custom span for this operation
        using var activity = _activitySource.StartActivity("CreateOrder");
        
        // Add tags to the span for filtering and searching
        activity?.SetTag("order.customer_id", request.CustomerId);
        activity?.SetTag("order.item_count", request.Items.Count);
        activity?.SetTag("order.total_amount", request.TotalAmount);

        _logger.LogInformation("Creating order for customer {CustomerId} with {ItemCount} items", 
            request.CustomerId, request.Items.Count);

        try
        {
            // Simulate order validation
            using var validationActivity = _activitySource.StartActivity("ValidateOrder");
            validationActivity?.SetTag("validation.type", "business_rules");
            
            await Task.Delay(50, cancellationToken); // Simulate validation work
            
            // Simulate order persistence
            using var persistActivity = _activitySource.StartActivity("PersistOrder");
            persistActivity?.SetTag("database.operation", "insert");
            persistActivity?.SetTag("database.table", "orders");
            
            await Task.Delay(100, cancellationToken); // Simulate database work

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Items = request.Items,
                TotalAmount = request.TotalAmount,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Add success metrics to the span
            activity?.SetTag("order.id", order.Id.ToString());
            activity?.SetStatus(ActivityStatusCode.Ok, "Order created successfully");

            _logger.LogInformation("Order {OrderId} created successfully", order.Id);

            return order;
        }
        catch (Exception ex)
        {
            // Record the error in the span
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}
// </CustomSpanExample>

// <DataModels>
public record CreateOrderRequest(
    string CustomerId,
    List<OrderItem> Items,
    decimal TotalAmount);

public record OrderItem(
    string ProductId,
    int Quantity,
    decimal Price);

public record Order
{
    public Guid Id { get; init; }
    public string CustomerId { get; init; } = string.Empty;
    public List<OrderItem> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
// </DataModels>