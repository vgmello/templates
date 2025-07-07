// TroubleshootingConfiguration.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;

    public class TroubleshootingConfiguration
    {
        public static void TroubleshootConfigurationConflicts()
        {
            var builder = Host.CreateApplicationBuilder();

            // <TroubleshootingConfiguration>
            // Configuration sources are processed in order. Last one wins.
            builder.Configuration.AddJsonFile("appsettings.json");
            builder.Configuration.AddEnvironmentVariables();
            // </TroubleshootingConfiguration>

            var app = builder.Build();
            app.Run();
        }
    }
}