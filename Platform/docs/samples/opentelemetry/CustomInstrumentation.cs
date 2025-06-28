using System.Diagnostics;
using System.Diagnostics.Metrics;

// #region CustomSources
public class BusinessOperationService
{
    private static readonly ActivitySource ActivitySource = new("MyCompany.BusinessOperations");
    private static readonly Meter Meter = new("MyCompany.BusinessOperations");
    
    private readonly Counter<int> _orderProcessedCounter;
    private readonly Histogram<double> _orderProcessingDuration;

    public BusinessOperationService()
    {
        _orderProcessedCounter = Meter.CreateCounter<int>(
            "orders_processed_total",
            description: "Total number of orders processed");

        _orderProcessingDuration = Meter.CreateHistogram<double>(
            "order_processing_duration_seconds",
            unit: "s",
            description: "Time taken to process orders");
    }

    public async Task ProcessBusinessOrderAsync(BusinessOrder order)
    {
        using var activity = ActivitySource.StartActivity("process-business-order");
        activity?.SetTag("order.id", order.Id.ToString());
        activity?.SetTag("order.customer_tier", order.CustomerTier);
        activity?.SetTag("order.priority", order.Priority.ToString());

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await ValidateBusinessRulesAsync(order);
            await ApplyDiscountsAsync(order);
            await ProcessPaymentAsync(order);
            await ScheduleDeliveryAsync(order);

            // Record successful processing
            _orderProcessedCounter.Add(1, 
                new KeyValuePair<string, object?>("status", "success"),
                new KeyValuePair<string, object?>("customer_tier", order.CustomerTier));

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("order.final_amount", order.FinalAmount);
        }
        catch (Exception ex)
        {
            _orderProcessedCounter.Add(1,
                new KeyValuePair<string, object?>("status", "error"),
                new KeyValuePair<string, object?>("error_type", ex.GetType().Name));

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _orderProcessingDuration.Record(stopwatch.Elapsed.TotalSeconds,
                new KeyValuePair<string, object?>("customer_tier", order.CustomerTier));
        }
    }

    private async Task ValidateBusinessRulesAsync(BusinessOrder order)
    {
        using var activity = ActivitySource.StartActivity("validate-business-rules");
        activity?.SetTag("rule.count", order.ApplicableRules.Count);

        foreach (var rule in order.ApplicableRules)
        {
            using var ruleActivity = ActivitySource.StartActivity("validate-rule");
            ruleActivity?.SetTag("rule.name", rule.Name);
            ruleActivity?.SetTag("rule.type", rule.Type);

            // Simulate rule validation
            await Task.Delay(10);
        }
    }

    private async Task ApplyDiscountsAsync(BusinessOrder order)
    {
        using var activity = ActivitySource.StartActivity("apply-discounts");
        
        var originalAmount = order.Amount;
        // Discount logic here
        await Task.Delay(20);
        
        activity?.SetTag("discount.original_amount", originalAmount);
        activity?.SetTag("discount.final_amount", order.FinalAmount);
        activity?.SetTag("discount.percentage", 
            ((originalAmount - order.FinalAmount) / originalAmount * 100));
    }

    private async Task ProcessPaymentAsync(BusinessOrder order)
    {
        using var activity = ActivitySource.StartActivity("process-payment");
        activity?.SetTag("payment.method", order.PaymentMethod);
        activity?.SetTag("payment.amount", order.FinalAmount);

        // Payment processing simulation
        await Task.Delay(100);
    }

    private async Task ScheduleDeliveryAsync(BusinessOrder order)
    {
        using var activity = ActivitySource.StartActivity("schedule-delivery");
        activity?.SetTag("delivery.type", order.DeliveryType);
        activity?.SetTag("delivery.address.city", order.DeliveryAddress.City);

        // Delivery scheduling simulation
        await Task.Delay(30);
    }
}
// #endregion

// Supporting types
public class BusinessOrder
{
    public Guid Id { get; set; }
    public string CustomerTier { get; set; } = string.Empty;
    public OrderPriority Priority { get; set; }
    public decimal Amount { get; set; }
    public decimal FinalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
    public Address DeliveryAddress { get; set; } = new();
    public List<BusinessRule> ApplicableRules { get; set; } = new();
}

public class BusinessRule
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class Address
{
    public string City { get; set; } = string.Empty;
}

public enum OrderPriority
{
    Low,
    Normal,
    High,
    Critical
}