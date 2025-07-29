// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;

namespace Billing.Invoices.Contracts.IntegrationEvents;

[EventTopic<Invoice>]
public record InvoiceFinalized(
    [PartitionKey] Guid TenantId,
    Guid InvoiceId,
    Guid CustomerId,
    string PublicInvoiceNumber,
    decimal FinalTotalAmount
);
