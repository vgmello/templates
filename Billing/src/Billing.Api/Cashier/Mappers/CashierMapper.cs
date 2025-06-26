// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api.Cashier.Mappers;

[Mapper]
public static partial class CashierMapper
{
    [MapperIgnoreSource(nameof(Contracts.Cashier.Models.Cashier.CashierPayments))]
    public static partial Billing.Cashier.Grpc.Models.Cashier ToGrpc(this Billing.Contracts.Cashier.Models.Cashier source);

    public static partial Billing.Cashier.Grpc.Models.Cashier ToGrpc(this GetCashiersQuery.Result source);
}
