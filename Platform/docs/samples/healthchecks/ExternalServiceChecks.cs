// ExternalServiceChecks.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class ExternalServiceChecks
    {
        public static void ExternalServices()
        {
            var builder = Host.CreateApplicationBuilder();

            // <ExternalServices>
            builder.Services.AddHealthChecks()
                .AddUrlGroup(new Uri("https://api.example.com/health"), name: "External API");
            // </ExternalServices>

            var app = builder.Build();
            app.Run();
        }
    }
}