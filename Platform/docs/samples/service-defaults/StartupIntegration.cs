// StartupIntegration.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class StartupIntegration
    {
        public static void IntegrateStartup()
        {
            var builder = Host.CreateApplicationBuilder();

            var app = builder.Build();

            // <StartupIntegration>
            app.Run(); // Enhanced RunAsync is part of ServiceDefaults
            // </StartupIntegration>
        }
    }
}