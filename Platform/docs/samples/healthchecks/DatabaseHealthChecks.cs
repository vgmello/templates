// DatabaseHealthChecks.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults.HealthChecks;

    public class DatabaseHealthChecks
    {
        public static void DatabaseChecks()
        {
            var builder = Host.CreateApplicationBuilder();

            // <DatabaseChecks>
            builder.Services.AddHealthChecks()
                .AddSqlServer("ConnectionStrings:DefaultConnection", name: "SQL Server");
            // </DatabaseChecks>

            var app = builder.Build();
            app.Run();
        }
    }
}