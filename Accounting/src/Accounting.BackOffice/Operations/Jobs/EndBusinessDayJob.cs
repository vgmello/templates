// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Contracts.Operations.IntegrationEvents;

namespace Accounting.BackOffice.Operations.Jobs;

public class EndBusinessDayJob(IMessageBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.PublishAsync(new BusinessDayEndedEvent());
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
