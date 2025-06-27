using Wolverine;
using Operations.Extensions.Abstractions.Messaging;

public record UserCreated(Guid UserId, string UserName);

public class UserProducer
{
    private readonly IMessageBus _messageBus;

    public UserProducer(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task PublishUserCreated(Guid userId, string userName)
    {
        var userCreatedEvent = new UserCreated(userId, userName);
        await _messageBus.PublishAsync(userCreatedEvent);
        Console.WriteLine($"Published UserCreated event for user: {userName}");
    }
}