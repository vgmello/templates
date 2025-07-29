// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;
using Riok.Mapperly.Abstractions;

namespace Billing.Invoices.Data;

[Mapper]
public static partial class DbMapper
{
    public static partial Invoice ToModel(this Entities.Invoice cashier);
}
