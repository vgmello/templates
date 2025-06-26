// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Contracts.Operations.IntegrationEvents;
using Accounting.Ledgers.Events;

namespace Accounting.BackOffice.Operations.Jobs;

public class EndBusinessDayJob(IMessageBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var businessDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await bus.PublishAsync(new LedgerBalanceCreated
            {
                LedgerId = Guid.NewGuid(),
                FinalBalance = 1000.50m,
                AsOfDate = businessDate
            });

            await bus.PublishAsync(new BusinessDayEnded
            {
                BusinessDate = businessDate,
                Market = "NYSE",
                Region = "North America"
            });

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
