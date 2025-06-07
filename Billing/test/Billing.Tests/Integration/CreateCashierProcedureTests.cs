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
        var command = new CreateCashierCommandHandler.InsertCashierCommand(Guid.NewGuid(), "Test", "test@example.com");

        var number = await CreateCashierCommandHandler.Handle(command, _fixture.DataSource, CancellationToken.None);

        number.ShouldBe(1);
    }
}
