using Wolverine;

namespace Operations.Samples.Customization;

public record MyMessage(string Text);

public static class MessageHandlers
{
    public static void Handle(MyMessage message)
    {
        Console.WriteLine(message.Text);
    }
}
