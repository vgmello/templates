// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Commands;
using Billing.Invoices.Contracts.IntegrationEvents;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Tests.Unit.Invoices;

public class CancelInvoiceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidInvoice_ShouldCancelAndReturnResult()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var invoiceId = Guid.NewGuid();

        messagingMock.InvokeCommandAsync(Arg.Any<CancelInvoiceCommandHandler.CancelInvoiceDbCommand>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CancelInvoiceCommand(invoiceId);

        // Act
        var (result, integrationEvent) = await CancelInvoiceCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Assert
        var invoice = result.Match(success => success, _ => null!);

        invoice.InvoiceId.ShouldBe(invoiceId);
        invoice.Status.ShouldBe("Cancelled");
        invoice.Name.ShouldBe("Cancelled Invoice");
        invoice.Amount.ShouldBe(0);
        invoice.Version.ShouldBe(99);

        // Verify integration event
        integrationEvent.ShouldNotBeNull();
        integrationEvent.ShouldBeOfType<InvoiceCancelled>();
        integrationEvent.InvoiceId.ShouldBe(invoiceId);

        // Verify that messaging was called with correct parameters
        await messagingMock.Received(1).InvokeCommandAsync(
            Arg.Is<CancelInvoiceCommandHandler.CancelInvoiceDbCommand>(cmd => cmd.InvoiceId == invoiceId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentInvoice_ShouldReturnValidationFailure()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var invoiceId = Guid.NewGuid();

        messagingMock.InvokeCommandAsync(Arg.Any<CancelInvoiceCommandHandler.CancelInvoiceDbCommand>(), Arg.Any<CancellationToken>())
            .Returns(0); // No rows affected, simulating non-existent invoice

        var command = new CancelInvoiceCommand(invoiceId);

        // Act
        var (result, integrationEvent) = await CancelInvoiceCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Assert
        var failures = result.Match(_ => null!, validationFailures => validationFailures);

        failures.ShouldNotBeNull();
        failures.Count.ShouldBe(1);
        failures[0].PropertyName.ShouldBe("InvoiceId");
        failures[0].ErrorMessage.ShouldBe("Invoice not found or cannot be cancelled");

        integrationEvent.ShouldBeNull();
    }
}
