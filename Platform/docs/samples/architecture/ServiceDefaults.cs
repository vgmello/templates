// ServiceDefaults.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class ServiceDefaults
    {
        public static void BasicServiceDefaults()
        {
            var builder = Host.CreateApplicationBuilder();

            // <ServiceDefaults>
            builder.AddServiceDefaults();
            // </ServiceDefaults>

            var app = builder.Build();
            app.Run();
        }
    }
}