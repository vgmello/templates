// StructuredLogging.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class StructuredLogging
    {
        public static void ConfigureLogging()
        {
            var builder = Host.CreateApplicationBuilder();

            // <StructuredLogging>
            builder.Logging.AddJsonConsole();
            // </StructuredLogging>

            var app = builder.Build();
            app.Run();
        }
    }
}