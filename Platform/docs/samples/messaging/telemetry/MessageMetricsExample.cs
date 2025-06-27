using System.Diagnostics.Metrics;

public class MessageMetricsExample
{
    public static void IllustrateMetrics()
    {
        var meter = new Meter("MyApplication.Messaging");
        var messagesSentCounter = meter.CreateCounter<long>("messaging.messages_sent", "messages", "Number of messages sent");
        var messageProcessingTimeHistogram = meter.CreateHistogram<double>("messaging.processing_time", "ms", "Message processing time");

        // Increment counter when a message is sent
        messagesSentCounter.Add(1);

        // Record processing time when a message is processed
        messageProcessingTimeHistogram.Record(150.5); // Example: 150.5 ms
    }
}