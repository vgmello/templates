// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Queries;
using Billing.Contracts.Cashiers.Models;

namespace Billing.Api.Cashiers.Mappers;

[Mapper]
public static partial class CashierMapper
{
    [MapperIgnoreSource(nameof(Cashier.CashierPayments))]
    public static partial Billing.Cashiers.Grpc.Models.Cashier ToGrpc(this Cashier source);

    public static partial Billing.Cashiers.Grpc.Models.Cashier ToGrpc(this GetCashiersQuery.Result source);
}
