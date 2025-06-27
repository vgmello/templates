// SourceGeneration.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;

    public class SourceGeneration
    {
        public static void UseSourceGenerators()
        {
            var builder = Host.CreateApplicationBuilder();

            // <SourceGeneration>
            // Source generators are compile-time features and don't require runtime code.
            // Example: DbCommandAttribute from Operations.Extensions.Abstractions.Dapper
            // </SourceGeneration>

            var app = builder.Build();
            app.Run();
        }
    }
}