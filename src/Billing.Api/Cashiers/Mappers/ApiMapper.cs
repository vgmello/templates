// Copyright (c) ABCDEG. All rights reserved.

using Billing.Api.Cashiers.Models;
using Billing.Cashiers.Commands;
using Billing.Cashiers.Queries;

namespace Billing.Api.Cashiers.Mappers;

[Mapper]
public static partial class ApiMapper
{
    public static partial CreateCashierCommand ToCommand(this CreateCashierRequest request, Guid tenantId);

    public static partial UpdateCashierCommand ToCommand(this UpdateCashierRequest request, Guid tenantId, Guid cashierId);

    public static partial GetCashiersQuery ToQuery(this GetCashiersRequest request, Guid tenantId);
}
