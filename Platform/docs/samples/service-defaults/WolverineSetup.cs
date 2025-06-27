// WolverineSetup.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using Wolverine;

    public class WolverineSetup
    {
        public static void ConfigureWolverine()
        {
            var builder = Host.CreateApplicationBuilder();

            // <WolverineSetup>
            builder.Host.UseWolverine();
            // </WolverineSetup>

            var app = builder.Build();
            app.Run();
        }
    }
}