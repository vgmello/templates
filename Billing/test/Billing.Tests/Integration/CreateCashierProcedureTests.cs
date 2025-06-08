using Billing.Cashier.Commands;
using Billing.Tests.Integration;
using System;
using Shouldly;

namespace Billing.Tests.Integration;

[CollectionDefinition("db", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<BillingDatabaseFixture>
{ }

[Collection("db")]
public class CreateCashierProcedureTests
{
    private readonly BillingDatabaseFixture _fixture;

    public CreateCashierProcedureTests(BillingDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InsertCashier_ReturnsNumber()
    {
        var cashier = new Billing.Cashier.Data.Entities.Cashier
        {
            CashierId = Guid.NewGuid(),
            Name = "Test",
            Email = "test@example.com"
        };

        var command = new CreateCashierCommandHandler.InsertCashierCommand(cashier);

        var number = await CreateCashierCommandHandler.Handle(command, CancellationToken.None);

        number.ShouldBe(1);
    }
}
