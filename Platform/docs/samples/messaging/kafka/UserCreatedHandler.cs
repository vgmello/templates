using Wolverine.Attributes;

public class UserCreatedHandler
{
    [Topic("my-app.usercreated")] // Specify the topic to consume from
    public void Handle(UserCreated message)
    {
        Console.WriteLine($"Received UserCreated event for user: {message.UserName} (ID: {message.UserId})");
        // Process the message
    }
}