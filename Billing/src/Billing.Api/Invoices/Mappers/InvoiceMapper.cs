// Copyright (c) ABCDEG. All rights reserved.

using Google.Protobuf.WellKnownTypes;

namespace Billing.Api.Invoices.Mappers;

[Mapper]
public static partial class InvoiceMapper
{
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.DueDate), nameof(Billing.Invoices.Grpc.Models.Invoice.DueDate), Use = nameof(DateTimeToTimestamp))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.CreatedDateUtc), nameof(Billing.Invoices.Grpc.Models.Invoice.CreatedDateUtc), Use = nameof(DateTimeToTimestamp))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.UpdatedDateUtc), nameof(Billing.Invoices.Grpc.Models.Invoice.UpdatedDateUtc), Use = nameof(DateTimeToTimestamp))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.CashierId), nameof(Billing.Invoices.Grpc.Models.Invoice.CashierId), Use = nameof(GuidToString))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.InvoiceId), nameof(Billing.Invoices.Grpc.Models.Invoice.InvoiceId), Use = nameof(GuidToString))]
    public static partial Billing.Invoices.Grpc.Models.Invoice ToGrpc(this Billing.Contracts.Invoices.Models.Invoice source);

    private static Timestamp DateTimeToTimestamp(DateTime dateTime)
    {
        return dateTime.ToTimestamp();
    }

    private static Timestamp? DateTimeToTimestamp(DateTime? dateTime)
    {
        return dateTime?.ToTimestamp();
    }

    private static string GuidToString(Guid guid)
    {
        return guid.ToString();
    }

    private static string GuidToString(Guid? guid)
    {
        return guid?.ToString() ?? string.Empty;
    }
}