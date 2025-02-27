// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Contracts.Operations.IntegrationEvents;
using MassTransit;

namespace Accounting.BackOffice.Operations.Jobs;

public class EndBusinessDayJob(IBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.Publish(new BusinessDayEndedEvent(), stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
