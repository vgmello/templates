using System.Diagnostics.Metrics;

// <SloSetup>
var meter = new Meter("WebService");
Histogram<double> requestLatency = meter.CreateHistogram<double>("request_latency_seconds");

public static void RecordLatency(double seconds) => requestLatency.Record(seconds);
// </SloSetup>
