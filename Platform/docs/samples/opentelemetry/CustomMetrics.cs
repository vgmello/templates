using System.Diagnostics.Metrics;

// <CustomCounters>
var meter = new Meter("Checkout");
Counter<int> orders = meter.CreateCounter<int>("orders_total");

public static void OrderProcessed() => orders.Add(1);
// </CustomCounters>
