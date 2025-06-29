// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.Models;
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
            DueDate = Timestamp.FromDateTime(source.DueDate ?? DateTime.MinValue),
            CreatedDateUtc = Timestamp.FromDateTime(source.CreatedDateUtc),
            UpdatedDateUtc = Timestamp.FromDateTime(source.UpdatedDateUtc),
            Version = source.Version
        };
}
