// HttpResilienceSetup.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Polly;
    using System;

    public class HttpResilienceSetup
    {
        public static void ConfigureHttpResilience()
        {
            var builder = Host.CreateApplicationBuilder();

            // <HttpResilienceSetup>
            builder.Services.AddHttpClient("MyClient")
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1)));
            // </HttpResilienceSetup>

            var app = builder.Build();
            app.Run();
        }
    }
}