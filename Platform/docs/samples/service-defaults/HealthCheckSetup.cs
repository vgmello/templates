// HealthCheckSetup.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class HealthCheckSetup
    {
        public static void ConfigureHealthChecks()
        {
            var builder = Host.CreateApplicationBuilder();

            // <HealthCheckSetup>
            builder.Services.AddHealthChecks();
            // </HealthCheckSetup>

            var app = builder.Build();
            app.Run();
        }
    }
}