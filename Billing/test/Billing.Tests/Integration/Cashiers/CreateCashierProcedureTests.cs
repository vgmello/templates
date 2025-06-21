// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Commands;
using Billing.Tests.Integration._Internal;
using Wolverine;
using CashierModel = Billing.Contracts.Cashier.Models.Cashier;

namespace Billing.Tests.Integration.Cashiers;

public class CreateCashierIntegrationTests(IntegrationTestFixture fixture) : IntegrationTest
{
    [Fact]
    public async Task CreateCashier_ShouldCreateCashierInDatabase()
    {
        // Arrange
        var command = new CreateCashierCommand("Integration Test Cashier", "integration@test.com");

        // Act
        var messageBus = fixture.Services.GetRequiredService<IMessageBus>();
        var handlerResult = await CreateCashierCommandHandler.Handle(command, messageBus, CancellationToken.None);
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
