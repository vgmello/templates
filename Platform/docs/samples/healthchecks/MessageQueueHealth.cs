// MessageQueueHealth.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class MessageQueueHealth
    {
        public static void QueueHealth()
        {
            var builder = Host.CreateApplicationBuilder();

            // <QueueHealth>
            builder.Services.AddHealthChecks()
                .AddRabbitMQ("amqp://localhost", name: "RabbitMQ");
            // </QueueHealth>

            var app = builder.Build();
            app.Run();
        }
    }
}