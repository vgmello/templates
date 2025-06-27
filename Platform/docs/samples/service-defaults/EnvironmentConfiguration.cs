// EnvironmentConfiguration.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;

    public class EnvironmentConfiguration
    {
        public static void ConfigureEnvironment()
        {
            var builder = Host.CreateApplicationBuilder();

            // <EnvironmentConfiguration>
            // Configuration is automatically loaded based on environment (e.g., appsettings.Development.json)
            // </EnvironmentConfiguration>

            var app = builder.Build();
            app.Run();
        }
    }
}