// JsonResponseFormat.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class JsonResponseFormat
    {
        public static void JsonFormat()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();

            // <JsonFormat>
            app.MapHealthChecks("/health-json", new HealthCheckOptions()
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalSeconds
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });
            // </JsonFormat>

            app.Run();
        }
    }
}