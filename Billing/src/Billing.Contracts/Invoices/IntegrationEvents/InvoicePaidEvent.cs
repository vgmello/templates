// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Contracts.Invoices.IntegrationEvents;

public record InvoicePaidEvent(Guid InvoiceId, decimal AmountPaid, DateTime PaymentDate);
