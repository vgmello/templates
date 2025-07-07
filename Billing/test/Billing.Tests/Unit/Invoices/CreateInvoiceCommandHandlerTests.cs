// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Commands;
using Billing.Invoices.Contracts.IntegrationEvents;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Tests.Unit.Invoices;

public class CreateInvoiceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateInvoiceAndReturnResult()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var cashierId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(30);

        messagingMock.InvokeCommandAsync(Arg.Any<CreateInvoiceCommandHandler.InsertInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CreateInvoiceCommand("Test Invoice", 100.50m, "USD", dueDate, cashierId);

        // Act
        var (result, integrationEvent) = await CreateInvoiceCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Assert
        var invoice = result.Match(success => success, _ => null!);

        invoice.ShouldNotBeNull();
        invoice.Name.ShouldBe("Test Invoice");
        invoice.Status.ShouldBe("Draft");
        invoice.Amount.ShouldBe(100.50m);
        invoice.Currency.ShouldBe("USD");
        invoice.DueDate.ShouldBe(dueDate);
        invoice.CashierId.ShouldBe(cashierId);
        invoice.InvoiceId.ShouldNotBe(Guid.Empty);
        invoice.Version.ShouldBe(1);

        // Verify integration event
        integrationEvent.ShouldNotBeNull();
        integrationEvent.ShouldBeOfType<InvoiceCreated>();
        integrationEvent.Invoice.InvoiceId.ShouldBe(invoice.InvoiceId);
        integrationEvent.Invoice.Name.ShouldBe(invoice.Name);

        // Verify that messaging was called with correct parameters
        await messagingMock.Received(1).InvokeCommandAsync(
            Arg.Is<CreateInvoiceCommandHandler.InsertInvoiceCommand>(cmd =>
                cmd.InvoiceId == invoice.InvoiceId &&
                cmd.Name == "Test Invoice" &&
                cmd.Status == "Draft" &&
                cmd.Amount == 100.50m &&
                cmd.Currency == "USD" &&
                cmd.DueDate == dueDate &&
                cmd.CashierId == cashierId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDefaults_ShouldUseDefaultValues()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        messagingMock.InvokeCommandAsync(Arg.Any<CreateInvoiceCommandHandler.InsertInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CreateInvoiceCommand("Test Invoice", 50.00m);

        // Act
        var handlerResult = await CreateInvoiceCommandHandler.Handle(command, messagingMock, CancellationToken.None);
        var result = handlerResult.Item1;

        // Assert
        var invoice = result.Match(success => success, _ => null!);

        invoice.Currency.ShouldBe("");
        invoice.DueDate.ShouldBeNull();
        invoice.CashierId.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueGuidForEachCall()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        messagingMock.InvokeCommandAsync(Arg.Any<CreateInvoiceCommandHandler.InsertInvoiceCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command1 = new CreateInvoiceCommand("Invoice 1", 100.00m);
        var command2 = new CreateInvoiceCommand("Invoice 2", 200.00m);

        // Act
        var handlerResult1 = await CreateInvoiceCommandHandler.Handle(command1, messagingMock, CancellationToken.None);
        var handlerResult2 = await CreateInvoiceCommandHandler.Handle(command2, messagingMock, CancellationToken.None);

        // Assert
        var invoice1 = handlerResult1.Item1.Match(success => success, _ => null!);
        var invoice2 = handlerResult2.Item1.Match(success => success, _ => null!);

        invoice1.InvoiceId.ShouldNotBe(invoice2.InvoiceId);
    }
}
