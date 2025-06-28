// ApplicationInsights.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class ApplicationInsights
    {
        public static void AppInsightsIntegration()
        {
            var builder = Host.CreateApplicationBuilder();

            // <AppInsightsIntegration>
            builder.Services.AddHealthChecks()
                .AddApplicationInsightsPublisher();
            // </AppInsightsIntegration>

            var app = builder.Build();
            app.Run();
        }
    }
}