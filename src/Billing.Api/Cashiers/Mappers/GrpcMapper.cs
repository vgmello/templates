// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Commands;
using Billing.Cashiers.Contracts.Models;
using Billing.Cashiers.Grpc;
using Billing.Cashiers.Queries;
using GrpcCashier = Billing.Cashiers.Grpc.Models.Cashier;

namespace Billing.Api.Cashiers.Mappers;

[Mapper]
public static partial class GrpcMapper
{
    [MapperIgnoreSource(nameof(Cashier.CashierPayments))]
    [MapperIgnoreSource(nameof(Cashier.Version))]
    public static partial GrpcCashier ToGrpc(this Cashier source);

    public static partial GrpcCashier ToGrpc(this GetCashiersQuery.Result source);

    public static partial CreateCashierCommand ToCommand(this CreateCashierRequest request, Guid tenantId);

    [MapperIgnoreSource(nameof(UpdateCashierRequest.CashierId))]
    public static partial UpdateCashierCommand ToCommand(this UpdateCashierRequest request, Guid tenantId, Guid cashierId);

    public static DeleteCashierCommand ToCommand(this DeleteCashierRequest request, Guid tenantId)
        => new(tenantId, Guid.Parse(request.CashierId));

    public static partial GetCashiersQuery ToQuery(this GetCashiersRequest request, Guid tenantId);

    public static GetCashierQuery ToQuery(this GetCashierRequest request, Guid tenantId) => new(tenantId, Guid.Parse(request.Id));

    private static string ToString(Guid guid) => guid.ToString();
}
