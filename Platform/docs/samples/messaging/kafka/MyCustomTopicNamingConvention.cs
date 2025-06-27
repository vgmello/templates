using Operations.ServiceDefaults.Messaging.Kafka;

// Example of a custom topic naming convention
public class MyCustomTopicNamingConvention : IKafkaTopicNamingConvention
{
    public string GetTopicName<T>()
    {
        return $"my-app.{typeof(T).Name.ToLowerInvariant()}";
    }
}

// Registering the custom convention
// builder.Services.AddSingleton<IKafkaTopicNamingConvention, MyCustomTopicNamingConvention>();