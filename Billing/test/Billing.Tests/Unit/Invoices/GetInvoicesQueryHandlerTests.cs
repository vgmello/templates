// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Queries;
using NSubstitute;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Tests.Unit.Invoices;

using InvoiceModel = Invoice;

public class GetInvoicesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnInvoices()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var cashierId1 = Guid.NewGuid();
        var cashierId2 = Guid.NewGuid();

        var expectedInvoices = new List<InvoiceModel>
        {
            new InvoiceModel
            {
                InvoiceId = Guid.NewGuid(),
                Name = "Invoice 1",
                Status = "Draft",
                Amount = 100.00m,
                Currency = "USD",
                CashierId = cashierId1,
                CreatedDateUtc = DateTime.UtcNow,
                UpdatedDateUtc = DateTime.UtcNow,
                Version = 1
            },
            new InvoiceModel
            {
                InvoiceId = Guid.NewGuid(),
                Name = "Invoice 2",
                Status = "Paid",
                Amount = 200.00m,
                Currency = "EUR",
                CashierId = cashierId2,
                CreatedDateUtc = DateTime.UtcNow.AddDays(-1),
                UpdatedDateUtc = DateTime.UtcNow,
                Version = 2
            }
        };

        messagingMock.InvokeQueryAsync(Arg.Any<GetInvoicesQueryHandler.GetInvoicesDbQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedInvoices);

        var query = new GetInvoicesQuery(10, 0, null);

        // Act
        var results = await GetInvoicesQueryHandler.Handle(query, messagingMock, CancellationToken.None);

        // Assert
        results.ShouldNotBeNull();
        results.Count().ShouldBe(2);

        var resultsList = results.ToList();
        resultsList[0].Name.ShouldBe("Invoice 1");
        resultsList[0].Status.ShouldBe("Draft");
        resultsList[1].Name.ShouldBe("Invoice 2");
        resultsList[1].Status.ShouldBe("Paid");

        // Verify that messaging was called with correct parameters
        await messagingMock.Received(1).InvokeQueryAsync(
            Arg.Is<GetInvoicesQueryHandler.GetInvoicesDbQuery>(cmd =>
                cmd.Limit == 10 &&
                cmd.Offset == 0 &&
                cmd.Status == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassStatusToQuery()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var expectedInvoices = new List<InvoiceModel>();

        messagingMock.InvokeQueryAsync(Arg.Any<GetInvoicesQueryHandler.GetInvoicesDbQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedInvoices);

        var query = new GetInvoicesQuery(20, 10, "Paid");

        // Act
        await GetInvoicesQueryHandler.Handle(query, messagingMock, CancellationToken.None);

        // Assert
        await messagingMock.Received(1).InvokeQueryAsync(
            Arg.Is<GetInvoicesQueryHandler.GetInvoicesDbQuery>(cmd =>
                cmd.Limit == 20 &&
                cmd.Offset == 10 &&
                cmd.Status == "Paid"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyCollection()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        var expectedInvoices = new List<InvoiceModel>();

        messagingMock.InvokeQueryAsync(Arg.Any<GetInvoicesQueryHandler.GetInvoicesDbQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedInvoices);

        var query = new GetInvoicesQuery();

        // Act
        var results = await GetInvoicesQueryHandler.Handle(query, messagingMock, CancellationToken.None);

        // Assert
        results.ShouldNotBeNull();
        results.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WithDefaults_ShouldUseDefaultValues()
    {
        // Arrange
        var messagingMock = Substitute.For<IMessageBus>();
        messagingMock.InvokeQueryAsync(Arg.Any<GetInvoicesQueryHandler.GetInvoicesDbQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<InvoiceModel>());

        var query = new GetInvoicesQuery(); // Using all defaults

        // Act
        await GetInvoicesQueryHandler.Handle(query, messagingMock, CancellationToken.None);

        // Assert
        await messagingMock.Received(1).InvokeQueryAsync(
            Arg.Is<GetInvoicesQueryHandler.GetInvoicesDbQuery>(cmd =>
                cmd.Limit == 50 &&
                cmd.Offset == 0 &&
                cmd.Status == null),
            Arg.Any<CancellationToken>());
    }
}
