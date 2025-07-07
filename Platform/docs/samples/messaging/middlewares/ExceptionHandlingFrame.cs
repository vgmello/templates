using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class ExceptionHandlingFrame : IChainableHandler
{
    public Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Execute the next step in the pipeline
            return context.Next(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            // Log the exception, send a dead-letter message, etc.
            return Task.CompletedTask;
        }
    }
}