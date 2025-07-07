// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;

namespace Billing.Invoices.Contracts.IntegrationEvents;

[EventTopic<Invoice>]
public record InvoiceCancelled(Guid InvoiceId);
