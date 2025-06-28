using System.Diagnostics.Metrics;

// <DashboardSetup>
var meter = new Meter("OrderProcessing");
Histogram<double> duration = meter.CreateHistogram<double>("order_duration_seconds");

public static void RecordDuration(double seconds) => duration.Record(seconds);
// </DashboardSetup>
