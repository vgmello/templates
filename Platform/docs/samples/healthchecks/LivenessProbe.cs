// LivenessProbe.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class LivenessProbe
    {
        public static void LivenessEndpoint()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.AddServiceDefaults();
            var app = builder.Build();

            // <LivenessEndpoint>
            app.MapHealthChecks("/status");
            // </LivenessEndpoint>

            app.Run();
        }
    }
}