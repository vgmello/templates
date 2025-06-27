// TaggedHealthChecks.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class TaggedHealthChecks
    {
        public static void TaggedChecks()
        {
            var builder = Host.CreateApplicationBuilder();

            // <TaggedChecks>
            builder.Services.AddHealthChecks()
                .AddSqlServer("ConnectionStrings:DefaultConnection", name: "SQL Server", tags: new[] { "db", "ready" })
                .AddUrlGroup(new Uri("https://api.example.com/health"), name: "External API", tags: new[] { "external", "live" });
            // </TaggedChecks>

            var app = builder.Build();
            app.Run();
        }
    }
}