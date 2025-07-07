// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Invoices.Contracts.DomainEvents;

public record InvoiceGenerated(Guid InvoiceId, decimal InvoiceAmount);
