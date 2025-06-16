// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Commands;
using Shouldly;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;
using CashierModel = Billing.Contracts.Cashier.Models.Cashier;

namespace Billing.Tests.Integration;

[CollectionDefinition("db", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<BillingDatabaseFixture>
{
}

[Collection("db")]
public class CreateCashierIntegrationTests(BillingDatabaseFixture fixture)
{
    [Fact]
    public async Task CreateCashier_ShouldCreateCashierInDatabase()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        messagingMock.InvokeCommandAsync(Arg.Any<CreateCashierCommandHandler.InsertCashierCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CreateCashierCommand("Integration Test Cashier", "integration@test.com");

        // Act
        var handlerResult = await CreateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None);
        var result = handlerResult.Item1;
        var integrationEvent = handlerResult.Item2;

        // Assert
        result.IsT0.ShouldBeTrue();

        var cashier = result.Value.ShouldBeOfType<CashierModel>();
        cashier.Name.ShouldBe("Integration Test Cashier");
        cashier.Email.ShouldBe("integration@test.com");
        cashier.CashierId.ShouldNotBe(Guid.Empty);

        integrationEvent.ShouldNotBeNull();
        integrationEvent.Cashier.CashierId.ShouldBe(cashier.CashierId);
    }
}
