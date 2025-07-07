// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Commands;
using Billing.Cashiers.Contracts.IntegrationEvents;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Tests.Unit.Cashier;

public class UpdateCashierCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCashier_ShouldUpdateAndReturnResult()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var cashierId = Guid.NewGuid();

        messagingMock.InvokeCommandAsync(Arg.Any<UpdateCashierCommandHandler.UpdateCashierDbCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new UpdateCashierCommand(cashierId, "Updated Name", "updated@example.com");

        // Act
        var (result, integrationEvent) = await UpdateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Assert
        var cashier = result.Match(success => success, _ => null!);

        cashier.CashierId.ShouldBe(cashierId);
        cashier.Name.ShouldBe("Updated Name");
        cashier.Email.ShouldBe("updated@example.com");

        // Verify integration event
        integrationEvent.ShouldNotBeNull();
        integrationEvent.ShouldBeOfType<CashierUpdated>();
        integrationEvent.CashierId.ShouldBe(cashierId);

        // Verify that messaging was called with correct parameters
        await messagingMock.Received(1).InvokeCommandAsync(
            Arg.Is<UpdateCashierCommandHandler.UpdateCashierDbCommand>(cmd =>
                cmd.CashierId == cashierId &&
                cmd.Name == "Updated Name" &&
                cmd.Email == "updated@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentCashier_ShouldReturnValidationFailure()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var cashierId = Guid.NewGuid();

        messagingMock.InvokeCommandAsync(Arg.Any<UpdateCashierCommandHandler.UpdateCashierDbCommand>(), Arg.Any<CancellationToken>())
            .Returns(0); // Simulating non-existent cashier

        var command = new UpdateCashierCommand(cashierId, "Updated Name", "updated@example.com");

        // Act
        var (result, integrationEvent) = await UpdateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Asser
        var failures = result.Match(_ => null!, validationFailures => validationFailures
        );

        failures.ShouldNotBeNull();
        failures.Count.ShouldBe(1);
        failures[0].PropertyName.ShouldBe("CashierId");
        failures[0].ErrorMessage.ShouldBe("Cashier not found");

        integrationEvent.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WithNullEmail_ShouldReturnNotUpdatedForEmail()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var cashierId = Guid.NewGuid();

        messagingMock.InvokeCommandAsync(Arg.Any<UpdateCashierCommandHandler.UpdateCashierDbCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new UpdateCashierCommand(cashierId, "Updated Name", null);

        // Act
        var handlerResult = await UpdateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None);
        var result = handlerResult.Item1;

        // Assert
        var cashier = result.Match(success => success, _ => null!);

        cashier.Email.ShouldBe("Not Updated");
    }
}
