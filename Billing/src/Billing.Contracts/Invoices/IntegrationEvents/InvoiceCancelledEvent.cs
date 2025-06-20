// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Contracts.Invoices.IntegrationEvents;

public record InvoiceCancelledEvent(Guid InvoiceId)
{
}