// Copyright (c) ABCDEG. All rights reserved.

using System.Text.Json.Serialization;

namespace Billing.Api.Invoices.Models;

public record CancelInvoiceRequest([property: JsonRequired] int Version);
