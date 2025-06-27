// DebuggingFailures.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;

    public class DebuggingFailures
    {
        public static void DebuggingTechniques()
        {
            var builder = Host.CreateApplicationBuilder();
            // <DebuggingTechniques>
            // Debugging techniques:
            // - Check application logs for health check errors
            // - Run health checks manually from a browser or tool
            // - Use a debugger to step through health check code
            // - Temporarily increase health check timeouts
            // </DebuggingTechniques>
            var app = builder.Build();
            app.Run();
        }
    }
}