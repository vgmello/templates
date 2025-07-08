// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.DomainEvents;
using Billing.Invoices.Contracts.IntegrationEvents;

namespace Billing.BackOffice.Messaging.AccountingInboxHandler;

public class BusinessDayEndedHandler(ILogger<BusinessDayEndedHandler> logger, IMessageBus messageBus)
{
    public async Task Handle(BusinessDayEnded businessDayEnded)
    {
        logger.LogInformation("Processing business day ended for {BusinessDate} in {Market}/{Region}",
            businessDayEnded.BusinessDate, businessDayEnded.Market, businessDayEnded.Region);

        var invoiceId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        await messageBus.PublishAsync(new InvoiceGenerated(invoiceId, 500.75m));

        await messageBus.PublishAsync(new InvoiceFinalized
        {
            InvoiceId = invoiceId,
            CustomerId = customerId,
            PublicInvoiceNumber = $"INV-{businessDayEnded.BusinessDate:yyyyMMdd}-{invoiceId:N}",
            FinalTotalAmount = 500.75m
        });
    }
}

// This declared in this file, for example purposes,
// in a real-world scenario is supposed to be declared in a different domain/project
[EventTopic("operations", domain: "accounting")]
public record BusinessDayEnded(DateTime BusinessDate, string Market, string Region);
