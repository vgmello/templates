using Serilog;

// #region StructuredLogs
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        // Structured logging with semantic properties
        _logger.LogInformation(
            "Processing order {OrderId} for customer {CustomerId} with amount {Amount:C}",
            order.Id,
            order.CustomerId,
            order.Amount);

        try
        {
            await ValidateOrderAsync(order);
            await ChargeCustomerAsync(order);
            await FulfillOrderAsync(order);

            // Log with structured data for analytics
            _logger.LogInformation(
                "Order {OrderId} completed successfully in {Duration:000}ms",
                order.Id,
                order.ProcessingDuration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            // Rich exception context
            _logger.LogError(ex,
                "Failed to process order {OrderId} for customer {CustomerId}. Reason: {FailureReason}",
                order.Id,
                order.CustomerId,
                ex.Message);
            throw;
        }
    }

    private async Task ValidateOrderAsync(Order order)
    {
        using var activity = _logger.BeginScope(
            "Validating order {OrderId}", order.Id);

        _logger.LogDebug("Checking inventory for {ProductCount} products", 
            order.Items.Count);

        // Validation logic here
        await Task.Delay(100);
    }

    private async Task ChargeCustomerAsync(Order order)
    {
        using var activity = _logger.BeginScope(
            "Charging customer {CustomerId} amount {Amount:C}",
            order.CustomerId, order.Amount);

        // Payment processing logic
        await Task.Delay(200);
    }

    private async Task FulfillOrderAsync(Order order)
    {
        using var activity = _logger.BeginScope(
            "Fulfilling order {OrderId}", order.Id);

        // Fulfillment logic
        await Task.Delay(300);
    }
}
// #endregion

// Supporting types
public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public TimeSpan ProcessingDuration { get; set; }
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}