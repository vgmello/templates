// Copyright (c) ABCDEG. All rights reserved.

using Billing.Infrastructure.Database;
using MassTransit;

namespace Billing.Cashier.Data.Persistence;

public class AddCashierDbCommand
{
    public Entities.Cashier Cashier { get; set; }
}

public class AddCashierDbCommandHandler(BillingDbContext context) : IConsumer<AddCashierDbCommand>
{
    public async Task Consume(ConsumeContext<AddCashierDbCommand> context1)
    {
        context.Cashiers.Add(context1.Message.Cashier);

        await context.SaveChangesAsync();
    }
}
