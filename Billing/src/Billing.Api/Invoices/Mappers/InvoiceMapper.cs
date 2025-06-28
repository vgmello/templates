// Copyright (c) ABCDEG. All rights reserved.

using Google.Protobuf.WellKnownTypes;

namespace Billing.Api.Invoices.Mappers;

[Mapper]
public static partial class InvoiceMapper
{
    [MapperIgnoreTarget(nameof(Billing.Invoices.Grpc.Models.Invoice.TenantId))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.DueDate), nameof(Billing.Invoices.Grpc.Models.Invoice.DueDate), Use = nameof(NullableDateTimeToTimestamp))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.CreatedDateUtc), nameof(Billing.Invoices.Grpc.Models.Invoice.CreatedDateUtc), Use = nameof(DateTimeToTimestamp))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.UpdatedDateUtc), nameof(Billing.Invoices.Grpc.Models.Invoice.UpdatedDateUtc), Use = nameof(DateTimeToTimestamp))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.CashierId), nameof(Billing.Invoices.Grpc.Models.Invoice.CashierId), Use = nameof(NullableGuidToString))]
    [MapProperty(nameof(Billing.Contracts.Invoices.Models.Invoice.InvoiceId), nameof(Billing.Invoices.Grpc.Models.Invoice.InvoiceId), Use = nameof(GuidToString))]
    public static partial Billing.Invoices.Grpc.Models.Invoice ToGrpc(this Billing.Contracts.Invoices.Models.Invoice source);

    public static Billing.Invoices.Grpc.Models.Invoice ToGrpcWithTenant(this Billing.Contracts.Invoices.Models.Invoice source, string tenantId = "")
    {
        var grpcInvoice = source.ToGrpc();
        grpcInvoice.TenantId = tenantId;
        return grpcInvoice;
    }

    private static Timestamp DateTimeToTimestamp(DateTime dateTime)
    {
        // Ensure DateTime is treated as UTC for gRPC conversion
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return utcDateTime.ToTimestamp();
    }

    private static Timestamp? NullableDateTimeToTimestamp(DateTime? dateTime)
    {
        if (dateTime == null) return null;
        // Ensure DateTime is treated as UTC for gRPC conversion
        var utcDateTime = dateTime.Value.Kind == DateTimeKind.Utc ? dateTime.Value : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
        return utcDateTime.ToTimestamp();
    }

    private static string GuidToString(Guid guid)
    {
        return guid.ToString();
    }

    private static string NullableGuidToString(Guid? guid)
    {
        return guid?.ToString() ?? string.Empty;
    }
}