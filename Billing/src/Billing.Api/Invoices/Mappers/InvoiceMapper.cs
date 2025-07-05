// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;
using Google.Protobuf.WellKnownTypes;

namespace Billing.Api.Invoices.Mappers;

[Mapper]
public static partial class InvoiceMapper
{
    public static Billing.Invoices.Grpc.Models.Invoice ToGrpc(this Invoice source) =>
        new()
        {
            TenantId = Guid.Empty.ToString(),
            InvoiceId = source.InvoiceId.ToString(),
            Name = source.Name,
            Amount = Convert.ToDouble(source.Amount),
            Status = source.Status,
            CashierId = source.CashierId?.ToString(),
            Currency = source.Currency ?? string.Empty,
            DueDate = source.DueDate?.ToTimestamp() ?? null,
            CreatedDateUtc = source.CreatedDateUtc.ToUniversalTime().ToTimestamp(),
            UpdatedDateUtc = source.UpdatedDateUtc.ToUniversalTime().ToTimestamp(),
            Version = source.Version
        };
}
