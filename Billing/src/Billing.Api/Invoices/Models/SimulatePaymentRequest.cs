// Copyright (c) ABCDEG. All rights reserved.

using System.Text.Json.Serialization;

namespace Billing.Api.Invoices.Models;

public record SimulatePaymentRequest(
    [property: JsonRequired] decimal Amount,
    string? Currency = "USD",
    string? PaymentMethod = "Credit Card",
    string? PaymentReference = null
);
