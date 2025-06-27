// CustomConfiguration.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Operations.ServiceDefaults;

    public class CustomConfiguration
    {
        public static void CustomLogging()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.AddServiceDefaults();

            // <CustomLogging>
            builder.Logging.SetMinimumLevel(LogLevel.Warning);
            // </CustomLogging>

            var app = builder.Build();
            app.Run();
        }

        public static void CustomTelemetry()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.AddServiceDefaults();

            // <CustomTelemetry>
            // Add custom OpenTelemetry sources here
            // </CustomTelemetry>

            var app = builder.Build();
            app.Run();
        }
    }
}