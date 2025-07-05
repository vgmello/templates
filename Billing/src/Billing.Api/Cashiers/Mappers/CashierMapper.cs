// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.Models;
using Billing.Cashiers.Queries;

namespace Billing.Api.Cashiers.Mappers;

[Mapper]
public static partial class CashierMapper
{
    [MapperIgnoreSource(nameof(Cashier.CashierPayments))]
    [MapProperty(nameof(Cashier.CashierId), nameof(Billing.Cashiers.Grpc.Models.Cashier.CashierId), StringFormat = "D")]
    public static partial Billing.Cashiers.Grpc.Models.Cashier ToGrpc(this Cashier source);

    public static partial Billing.Cashiers.Grpc.Models.Cashier ToGrpc(this GetCashiersQuery.Result source);
}
