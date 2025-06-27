// CustomWolverine.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;
    using Wolverine;

    public class CustomWolverine
    {
        public static void ConfigureCustomWolverine()
        {
            var builder = Host.CreateApplicationBuilder();

            // <CustomWolverine>
            builder.Host.UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(CustomWolverine).Assembly);
            });
            // </CustomWolverine>

            var app = builder.Build();
            app.Run();
        }
    }
}