using System.Diagnostics;
using Wolverine.Runtime;

public class MessageTracingExample
{
    public static void IllustrateTracing()
    {
        var activitySource = new ActivitySource("MyApplication.Messaging");

        // When a message is sent or received, an Activity is created and propagated
        using (var activity = activitySource.StartActivity("MessageSend", ActivityKind.Producer))
        {
            activity?.SetTag("message.type", "OrderCreated");
            activity?.SetTag("message.id", Guid.NewGuid());
            // ... send message
        }

        using (var activity = activitySource.StartActivity("MessageReceive", ActivityKind.Consumer))
        {
            activity?.SetTag("message.type", "OrderCreated");
            activity?.SetTag("message.id", Guid.NewGuid());
            // ... process message
        }
    }
}