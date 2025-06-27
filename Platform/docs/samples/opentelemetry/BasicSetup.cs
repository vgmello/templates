using Operations.ServiceDefaults;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// #region BasicOTel
// Add service defaults which includes OpenTelemetry
builder.AddServiceDefaults();

// OpenTelemetry is automatically configured with:
// - Distributed tracing across service boundaries
// - Metrics collection for performance monitoring
// - Automatic instrumentation for ASP.NET Core and HTTP clients
// - OTLP export to observability platforms
// - Logging integration with trace correlation
// #endregion

var app = builder.Build();

// #region TelemetryUsage
// Use OpenTelemetry throughout your application
app.MapGet("/orders/{id:guid}", async (Guid id, OrderService orderService) =>
{
    using var activity = Activity.Current?.Source.StartActivity("process-order");
    activity?.SetTag("order.id", id.ToString());
    
    var order = await orderService.GetOrderAsync(id);
    return Results.Ok(order);
});
// #endregion

await app.RunAsync();

public class OrderService
{
    public async Task<Order> GetOrderAsync(Guid id)
    {
        using var activity = Activity.Current?.Source.StartActivity("get-order-from-database");
        activity?.SetTag("order.id", id.ToString());
        
        // Simulate database call
        await Task.Delay(50);
        
        return new Order { Id = id };
    }
}

public class Order
{
    public Guid Id { get; set; }
}