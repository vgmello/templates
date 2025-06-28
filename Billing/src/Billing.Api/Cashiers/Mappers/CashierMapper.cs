// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Queries;

namespace Billing.Api.Cashiers.Mappers;

[Mapper]
public static partial class CashierMapper
{
    [MapperIgnoreSource(nameof(Contracts.Cashiers.Models.Cashier.CashierPayments))]
    public static partial Billing.Cashiers.Grpc.Models.Cashier ToGrpc(this Contracts.Cashiers.Models.Cashier source);

    public static partial Billing.Cashiers.Grpc.Models.Cashier ToGrpc(this GetCashiersQuery.Result source);
}
