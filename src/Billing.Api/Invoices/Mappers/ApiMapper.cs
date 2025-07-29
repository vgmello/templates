// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Queries;

namespace Billing.Api.Invoices.Mappers;

[Mapper]
public static partial class ApiMapper
{
    public static partial GetInvoicesQuery ToQuery(this Models.GetInvoicesRequest request, Guid tenantId);
}
