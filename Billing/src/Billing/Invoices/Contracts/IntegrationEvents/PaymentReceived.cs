// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Invoices.Contracts.IntegrationEvents;

[EventTopic("payment-received")]
public record PaymentReceived(
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string PaymentReference,
    DateTime ReceivedDate
);
