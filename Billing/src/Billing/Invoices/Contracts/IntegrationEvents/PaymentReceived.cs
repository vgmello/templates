// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Invoices.Contracts.IntegrationEvents;

public record PaymentReceived(
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string PaymentReference,
    DateTime ReceivedDate
);
