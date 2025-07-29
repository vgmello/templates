// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Data.Entities;
using Billing.Invoices.Data.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace Billing.Core.Data;

public class BillingDb(DataOptions<BillingDb> options) : DataConnection(options.Options)
{
    public ITable<Cashier> Cashiers => this.GetTable<Cashier>();
    public ITable<Invoice> Invoices => this.GetTable<Invoice>();
}
