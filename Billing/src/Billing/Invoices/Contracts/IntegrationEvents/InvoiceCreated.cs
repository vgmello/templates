// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;

namespace Billing.Invoices.Contracts.IntegrationEvents;

public record InvoiceCreated(Invoice Invoice);
