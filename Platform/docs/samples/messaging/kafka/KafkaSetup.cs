using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Messaging.Kafka;

public class KafkaSetup
{
    public static void ConfigureKafka()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddServiceDefaults();

        // Add Kafka specific configuration
        builder.Services.AddKafka();

        var app = builder.Build();
        app.Run();
    }
}