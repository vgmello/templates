// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Commands;
using Billing.Invoices.Contracts.IntegrationEvents;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Tests.Unit.Invoices;

public class MarkInvoiceAsPaidCommandHandlerTests
{
    [Fact(Skip = "Not ready yet")]
    public async Task Handle_WithValidInvoice_ShouldMarkAsPaidAndReturnResult()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var invoiceId = Guid.NewGuid();
        var amountPaid = 150.75m;
        var paymentDate = DateTime.UtcNow;
        var expectedAffectedRecords = 1;

        messagingMock.InvokeCommandAsync(Arg.Any<MarkInvoiceAsPaidCommandHandler.MarkInvoiceAsPaidDbCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedAffectedRecords);

        var command = new MarkInvoiceAsPaidCommand(invoiceId, amountPaid, paymentDate);

        // Act
        var (result, integrationEvent) = await MarkInvoiceAsPaidCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Assert
        var invoice = result.Match(success => success, _ => null!);

        invoice.InvoiceId.ShouldBe(invoiceId);
        invoice.Status.ShouldBe("Paid");
        invoice.Amount.ShouldBe(amountPaid);

        // Verify integration event
        integrationEvent.ShouldNotBeNull();
        integrationEvent.ShouldBeOfType<InvoicePaid>();
        integrationEvent.InvoiceId.ShouldBe(invoiceId);
        integrationEvent.AmountPaid.ShouldBe(amountPaid);
        integrationEvent.PaymentDate.ShouldBe(paymentDate);

        // Verify that messaging was called with correct parameters
        await messagingMock.Received(1).InvokeCommandAsync(
            Arg.Is<MarkInvoiceAsPaidCommandHandler.MarkInvoiceAsPaidDbCommand>(cmd =>
                cmd.InvoiceId == invoiceId &&
                cmd.AmountPaid == amountPaid &&
                cmd.PaymentDate == paymentDate),
            Arg.Any<CancellationToken>());
    }

    [Fact(Skip = "Not ready yet")]
    public async Task Handle_WithNullPaymentDate_ShouldUseUtcNow()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var invoiceId = Guid.NewGuid();
        const decimal amountPaid = 100.00m;

        messagingMock.InvokeCommandAsync(
                Arg.Any<MarkInvoiceAsPaidCommandHandler.MarkInvoiceAsPaidDbCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new MarkInvoiceAsPaidCommand(invoiceId, amountPaid);

        // Act
        var handlerResult = await MarkInvoiceAsPaidCommandHandler.Handle(command, messagingMock, CancellationToken.None);
        var integrationEvent = handlerResult.Item2;

        // Assert
        integrationEvent.ShouldNotBeNull();
        integrationEvent.PaymentDate.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task Handle_WithNonExistentInvoice_ShouldReturnValidationFailure()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var invoiceId = Guid.NewGuid();

        messagingMock.InvokeCommandAsync(Arg.Any<MarkInvoiceAsPaidCommandHandler.MarkInvoiceAsPaidDbCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(0); // Simulate no rows affected

        var command = new MarkInvoiceAsPaidCommand(invoiceId, 100.00m);

        // Act
        var (result, integrationEvent) = await MarkInvoiceAsPaidCommandHandler.Handle(command, messagingMock, CancellationToken.None);

        // Assert
        var failures = result.Match(_ => null!, validationFailures => validationFailures);

        failures.ShouldNotBeNull();
        failures.Count.ShouldBe(1);
        failures[0].PropertyName.ShouldBe("InvoiceId");
        failures[0].ErrorMessage.ShouldBe("Invoice not found or already paid");

        integrationEvent.ShouldBeNull();
    }
}
