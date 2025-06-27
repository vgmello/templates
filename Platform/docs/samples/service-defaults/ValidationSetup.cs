// ValidationSetup.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using FluentValidation;

    public class ValidationSetup
    {
        public static void ConfigureValidation()
        {
            var builder = Host.CreateApplicationBuilder();

            // <ValidationSetup>
            builder.Services.AddValidatorsFromAssemblyContaining<ValidationSetup>();
            // </ValidationSetup>

            var app = builder.Build();
            app.Run();
        }
    }
}