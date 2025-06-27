// DomainAssemblyDiscovery.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class DomainAssemblyDiscovery
    {
        public static void DiscoverAssemblies()
        {
            var builder = Host.CreateApplicationBuilder();

            // <DomainAssemblyDiscovery>
            // Assemblies marked with [DomainAssembly] are automatically discovered.
            // </DomainAssemblyDiscovery>

            var app = builder.Build();
            app.Run();
        }
    }
}