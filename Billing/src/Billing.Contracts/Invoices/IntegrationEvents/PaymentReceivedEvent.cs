// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Contracts.Invoices.IntegrationEvents;

public record PaymentReceivedEvent(
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string PaymentReference,
    DateTime ReceivedDate
);
