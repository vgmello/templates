// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api.Cashier.Mappers;

[Mapper]
public static partial class CashierMapper
{
    [MapperIgnoreSource(nameof(Contracts.Cashier.Models.Cashier.CashierPayments))]
    public static partial Billing.Cashier.Grpc.Models.Cashier ToGrpc(this Billing.Contracts.Cashier.Models.Cashier source);

    public static Billing.Cashier.Grpc.Models.Cashier ToGrpc(this GetCashiersQuery.Result source)
    {
        return new Billing.Cashier.Grpc.Models.Cashier
        {
            TenantId = string.Empty, // Set empty string instead of null GUID
            CashierId = source.CashierId.ToString(),
            Name = source.Name,
            Email = source.Email
        };
    }
}
