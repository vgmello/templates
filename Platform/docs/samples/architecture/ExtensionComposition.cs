// ExtensionComposition.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;
    using Operations.ServiceDefaults.Api;

    public class ExtensionComposition
    {
        public static void ComposeExtensions()
        {
            var builder = Host.CreateApplicationBuilder();

            // <ExtensionComposition>
            builder.AddServiceDefaults()
                   .AddServiceDefaultsApi();
            // </ExtensionComposition>

            var app = builder.Build();
            app.Run();
        }
    }
}