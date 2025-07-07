// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Queries;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Tests.Unit.Invoices;

using InvoiceModel = Invoice;

public class GetInvoiceQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithValidId_ShouldReturnInvoice()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var invoiceId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();

        var expectedInvoice = new InvoiceModel
        {
            InvoiceId = invoiceId,
            Name = "Test Invoice",
            Status = "Draft",
            Amount = 100.50m,
            Currency = "USD",
            CashierId = cashierId,
            CreatedDateUtc = DateTime.UtcNow,
            UpdatedDateUtc = DateTime.UtcNow,
            Version = 1
        };

        messagingMock.InvokeQueryAsync(Arg.Any<GetInvoiceQueryHandler.GetInvoiceDbQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedInvoice);

        var query = new GetInvoiceQuery(invoiceId);

        // Act
        var result = await GetInvoiceQueryHandler.Handle(query, messagingMock, CancellationToken.None);

        // Assert
        var invoice = result.Match(success => success, _ => null!);

        invoice.InvoiceId.ShouldBe(invoiceId);
        invoice.Name.ShouldBe("Test Invoice");
        invoice.Status.ShouldBe("Draft");
        invoice.Amount.ShouldBe(100.50m);
        invoice.Currency.ShouldBe("USD");
        invoice.CashierId.ShouldBe(cashierId);

        // Verify that messaging was called with correct parameters
        await messagingMock.Received(1).InvokeQueryAsync(
            Arg.Is<GetInvoiceQueryHandler.GetInvoiceDbQuery>(cmd => cmd.InvoiceId == invoiceId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnValidationFailure()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var invoiceId = Guid.NewGuid();

        messagingMock.InvokeQueryAsync(Arg.Any<GetInvoiceQueryHandler.GetInvoiceDbQuery>(), Arg.Any<CancellationToken>())
            .Returns((InvoiceModel?)null);

        var query = new GetInvoiceQuery(invoiceId);

        // Act
        var result = await GetInvoiceQueryHandler.Handle(query, messagingMock, CancellationToken.None);

        // Assert
        var failures = result.Match(_ => null!, validationFailures => validationFailures);

        failures.ShouldNotBeNull();
        failures.Count.ShouldBe(1);
        failures[0].PropertyName.ShouldBe("Id");
        failures[0].ErrorMessage.ShouldBe("Invoice not found");
    }
}
