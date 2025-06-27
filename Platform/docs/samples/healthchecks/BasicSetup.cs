// BasicSetup.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class BasicSetup
    {
        public static void BasicHealthChecks()
        {
            var builder = Host.CreateApplicationBuilder();

            // <BasicHealthChecks>
            builder.AddServiceDefaults();
            // </BasicHealthChecks>

            var app = builder.Build();
            app.Run();
        }
    }
}