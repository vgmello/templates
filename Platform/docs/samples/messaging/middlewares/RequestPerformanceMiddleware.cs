using System.Diagnostics;
using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class RequestPerformanceMiddleware : IChainableHandler
{
    public async Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await context.Next(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine($"Message {context.Envelope.MessageType.Name} processed in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}