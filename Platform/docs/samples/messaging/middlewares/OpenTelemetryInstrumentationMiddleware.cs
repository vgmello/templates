using System.Diagnostics;
using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class OpenTelemetryInstrumentationMiddleware : IChainableHandler
{
    private readonly ActivitySource _activitySource;

    public OpenTelemetryInstrumentationMiddleware(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    }

    public async Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        using (var activity = _activitySource.StartActivity($"ProcessMessage: {context.Envelope.MessageType.Name}"))
        {
            activity?.SetTag("message.type", context.Envelope.MessageType.Name);
            await context.Next(cancellationToken);
        }
    }
}