// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api.Invoices.Mappers;

[Mapper]
public static partial class InvoiceMapper
{
    [MapperIgnoreTarget(nameof(Billing.Invoices.Grpc.Models.Invoice.TenantId))]
    public static Billing.Invoices.Grpc.Models.Invoice ToGrpc(this Billing.Contracts.Invoices.Models.Invoice source)
    {
    }

    public static Billing.Invoices.Grpc.Models.Invoice ToGrpcWithTenant(this Billing.Contracts.Invoices.Models.Invoice source,
        string tenantId = "")
    {
        var grpcInvoice = source.ToGrpc();
        grpcInvoice.TenantId = tenantId;

        return grpcInvoice;
    }
}
