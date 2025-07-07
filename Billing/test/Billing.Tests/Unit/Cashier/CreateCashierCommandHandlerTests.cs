// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Commands;
using Billing.Cashiers.Contracts.IntegrationEvents;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Tests.Unit.Cashier;

public class CreateCashierCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCashierAndReturnResult()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();

        messagingMock.InvokeCommandAsync(Arg.Any<CreateCashierCommandHandler.InsertCashierCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CreateCashierCommand(Guid.Empty, "John Doe", "john.doe@example.com");

        // Act
        var (result, integrationEvent) = await CreateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Assert
        var cashier = result.Match(success => success, _ => null!);

        cashier.ShouldNotBeNull();
        cashier.Name.ShouldBe("John Doe");
        cashier.Email.ShouldBe("john.doe@example.com");
        cashier.CashierId.ShouldNotBe(Guid.Empty);

        // Verify integration event
        integrationEvent.ShouldNotBeNull();
        integrationEvent.ShouldBeOfType<CashierCreated>();
        integrationEvent.Cashier.CashierId.ShouldBe(cashier.CashierId);
        integrationEvent.Cashier.Name.ShouldBe(cashier.Name);
        integrationEvent.Cashier.Email.ShouldBe(cashier.Email);

        // Verify that messaging was called with correct parameters
        await messagingMock.Received(1).InvokeCommandAsync(
            Arg.Is<CreateCashierCommandHandler.InsertCashierCommand>(cmd =>
                cmd.CashierId == cashier.CashierId &&
                cmd.Name == "John Doe" &&
                cmd.Email == "john.doe@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueGuidForEachCall()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        messagingMock.InvokeCommandAsync(Arg.Any<CreateCashierCommandHandler.InsertCashierCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command1 = new CreateCashierCommand(Guid.Empty, "Cashier 1", "cashier1@test.com");
        var command2 = new CreateCashierCommand(Guid.Empty, "Cashier 2", "cashier2@test.com");

        // Act
        var handlerResult1 = await CreateCashierCommandHandler.Handle(command1, messagingMock, CancellationToken.None);
        var handlerResult2 = await CreateCashierCommandHandler.Handle(command2, messagingMock, CancellationToken.None);

        // Assert
        var cashier1 = handlerResult1.Item1.Match(success => success, _ => null!);
        var cashier2 = handlerResult2.Item1.Match(success => success, _ => null!);

        cashier1.CashierId.ShouldNotBe(cashier2.CashierId);
    }

    [Fact]
    public async Task Handle_WithErrorInName_ShouldThrowException()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var command = new CreateCashierCommand(Guid.Empty, "error", "test@example.com");

        // Act & Assert
        var exception = await Should.ThrowAsync<DivideByZeroException>(async () =>
            await CreateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None));

        exception.Message.ShouldBe("Forced test unhandled exception to simulate error scenarios");
    }
}
