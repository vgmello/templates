// HierarchicalConfig.cs
namespace SampleApp
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    public class HierarchicalConfig
    {
        public static void ConfigureHierarchically()
        {
            var builder = Host.CreateApplicationBuilder();

            // <HierarchicalConfig>
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            // </HierarchicalConfig>

            var app = builder.Build();
            app.Run();
        }
    }
}