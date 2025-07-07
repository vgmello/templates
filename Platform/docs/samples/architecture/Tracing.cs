// Tracing.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using System.Diagnostics;

    public class Tracing
    {
        public static void ConfigureTracing()
        {
            var builder = Host.CreateApplicationBuilder();

            // <Tracing>
            var activitySource = new ActivitySource("SampleApp.Tracing");
            using (var activity = activitySource.StartActivity("SampleOperation"))
            {
                activity?.SetTag("http.method", "GET");
            }
            // </Tracing>

            var app = builder.Build();
            app.Run();
        }
    }
}