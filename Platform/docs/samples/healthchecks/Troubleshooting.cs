// Troubleshooting.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;

    public class Troubleshooting
    {
        public static void CommonIssues()
        {
            var builder = Host.CreateApplicationBuilder();
            // <CommonIssues>
            // Common issues include:
            // - Incorrect connection strings
            // - Firewall blocking access
            // - Service not running
            // - Health check timeout too short
            // </CommonIssues>
            var app = builder.Build();
            app.Run();
        }
    }
}