// EnvironmentConfig.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;

    public class EnvironmentConfig
    {
        public static void ConfigureEnvironment()
        {
            var builder = Host.CreateApplicationBuilder();

            // <EnvironmentConfig>
            // Configuration is automatically loaded based on environment (e.g., appsettings.Development.json)
            // </EnvironmentConfig>

            var app = builder.Build();
            app.Run();
        }
    }
}