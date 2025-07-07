// StatusCodes.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System.Net;

    public class StatusCodes
    {
        public static void StatusResponses()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();

            // <StatusResponses>
            app.MapHealthChecks("/health-status-codes", new HealthCheckOptions()
            {
                ResultStatusCodes = {
                    [HealthStatus.Healthy] = (int)HttpStatusCode.OK,
                    [HealthStatus.Degraded] = (int)HttpStatusCode.OK,
                    [HealthStatus.Unhealthy] = (int)HttpStatusCode.ServiceUnavailable
                }
            });
            // </StatusResponses>

            app.Run();
        }
    }
}