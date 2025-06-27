// LoggingSetup.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class LoggingSetup
    {
        public static void ConfigureLogging()
        {
            var builder = Host.CreateApplicationBuilder();

            // <LoggingSetup>
            builder.Logging.AddConsole();
            // </LoggingSetup>

            var app = builder.Build();
            app.Run();
        }
    }
}