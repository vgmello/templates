// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Commands;
using Billing.Contracts.Cashier.IntegrationEvents;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;
using Operations.Extensions;
using Shouldly;
using CashierModel = Billing.Contracts.Cashier.Models.Cashier;

namespace Billing.Tests.Unit;

public class CreateCashierCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCashierAndReturnResult()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var expectedAffectedRecords = 1;
        
        messagingMock.InvokeCommandAsync(Arg.Any<CreateCashierCommandHandler.InsertCashierCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedAffectedRecords);

        var command = new CreateCashierCommand("John Doe", "john.doe@example.com");

        // Act
        var handlerResult = await CreateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None);
        var result = handlerResult.Item1;
        var integrationEvent = handlerResult.Item2;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var cashier = result.Value;
        
        cashier.Name.ShouldBe("John Doe");
        cashier.Email.ShouldBe("john.doe@example.com");
        cashier.CashierId.ShouldNotBe(Guid.Empty);

        // Verify integration event
        integrationEvent.ShouldNotBeNull();
        integrationEvent.ShouldBeOfType<CashierCreatedEvent>();
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
    public async Task Handle_WithNullEmail_ShouldReturnEmptyStringForEmail()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        messagingMock.InvokeCommandAsync(Arg.Any<CreateCashierCommandHandler.InsertCashierCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CreateCashierCommand("Jane Doe", null);

        // Act
        var handlerResult = await CreateCashierCommandHandler.Handle(command, messagingMock, CancellationToken.None);
        var result = handlerResult.Item1;
        var integrationEvent = handlerResult.Item2;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var cashier = result.Value;
        
        cashier.Name.ShouldBe("Jane Doe");
        cashier.Email.ShouldBe(string.Empty);
        
        integrationEvent.Cashier.Email.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueGuidForEachCall()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        messagingMock.InvokeCommandAsync(Arg.Any<CreateCashierCommandHandler.InsertCashierCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command1 = new CreateCashierCommand("Cashier 1", "cashier1@test.com");
        var command2 = new CreateCashierCommand("Cashier 2", "cashier2@test.com");

        // Act
        var handlerResult1 = await CreateCashierCommandHandler.Handle(command1, messagingMock, CancellationToken.None);
        var handlerResult2 = await CreateCashierCommandHandler.Handle(command2, messagingMock, CancellationToken.None);

        // Assert
        handlerResult1.Item1.Value.CashierId.ShouldNotBe(handlerResult2.Item1.Value.CashierId);
    }
}