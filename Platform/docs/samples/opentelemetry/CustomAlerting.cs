using System.Diagnostics.Metrics;

// <AlertSetup>
var meter = new Meter("Payments");
Counter<int> failed = meter.CreateCounter<int>("payments_failed_total");

public static void RecordFailed() => failed.Add(1);
// </AlertSetup>
