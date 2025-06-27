// DatabaseCluster.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class DatabaseCluster
    {
        public static void ClusterHealth()
        {
            var builder = Host.CreateApplicationBuilder();

            // <ClusterHealth>
            builder.Services.AddHealthChecks()
                .AddSqlServer("ConnectionStrings:PrimaryDb", name: "Primary DB")
                .AddSqlServer("ConnectionStrings:SecondaryDb", name: "Secondary DB");
            // </ClusterHealth>

            var app = builder.Build();
            app.Run();
        }
    }
}