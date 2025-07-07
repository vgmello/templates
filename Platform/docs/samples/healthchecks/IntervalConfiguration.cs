// IntervalConfiguration.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System;

    public class IntervalConfiguration
    {
        public static void IntervalSettings()
        {
            var builder = Host.CreateApplicationBuilder();

            // <IntervalSettings>
            builder.Services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(10);
            });
            // </IntervalSettings>

            var app = builder.Build();
            app.Run();
        }
    }
}