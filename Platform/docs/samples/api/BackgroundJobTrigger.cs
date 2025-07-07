using System;
using Wolverine;

public static class BackgroundJobTrigger
{
    // <BackgroundJobTrigger>
    public static void EnqueueCleanup(IMessageBus bus)
    {
        bus.Schedule(new CleanupExpiredRecords(), TimeSpan.Zero);
    }
    // </BackgroundJobTrigger>
}

public record CleanupExpiredRecords();
