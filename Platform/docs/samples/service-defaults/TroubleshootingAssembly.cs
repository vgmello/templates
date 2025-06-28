// TroubleshootingAssembly.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class TroubleshootingAssembly
    {
        public static void TroubleshootAssemblyDiscovery()
        {
            var builder = Host.CreateApplicationBuilder();

            // <TroubleshootingAssembly>
            // Ensure your domain assemblies have the [DomainAssembly] attribute.
            // </TroubleshootingAssembly>

            var app = builder.Build();
            app.Run();
        }
    }
}