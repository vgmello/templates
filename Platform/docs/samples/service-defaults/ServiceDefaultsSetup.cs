// ServiceDefaultsSetup.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class ServiceDefaultsSetup
    {
        public static void ServiceDefaultsConfiguration()
        {
            var builder = Host.CreateApplicationBuilder();

            // <ServiceDefaultsConfiguration>
            builder.AddServiceDefaults();
            // </ServiceDefaultsConfiguration>

            var app = builder.Build();
            app.Run();
        }
    }
}