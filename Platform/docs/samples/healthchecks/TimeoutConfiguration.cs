// TimeoutConfiguration.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;

    public class TimeoutConfiguration
    {
        public static void TimeoutSettings()
        {
            var builder = Host.CreateApplicationBuilder();

            // <TimeoutSettings>
            builder.Services.AddHealthChecks()
                .AddSqlServer("ConnectionStrings:DefaultConnection", name: "SQL Server", timeout: TimeSpan.FromSeconds(5));
            // </TimeoutSettings>

            var app = builder.Build();
            app.Run();
        }
    }
}