// ReadinessProbe.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class ReadinessProbe
    {
        public static void ReadinessEndpoint()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.AddServiceDefaults();
            var app = builder.Build();

            // <ReadinessEndpoint>
            app.MapHealthChecks("/health/internal");
            // </ReadinessEndpoint>

            app.Run();
        }
    }
}