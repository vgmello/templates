// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.Models;

namespace Billing.Contracts.Invoices.IntegrationEvents;

public record InvoiceCreated(Invoice Invoice);
