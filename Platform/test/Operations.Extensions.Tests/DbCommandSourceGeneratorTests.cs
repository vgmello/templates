// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Operations.Extensions.Tests;

public partial class DbCommandSourceGeneratorTests
{
    [DbCommand(sp: "billing.create_cashier", nonQuery: true, paramsCase: DbParamsCase.SnakeCase)]
    public partial record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;
}
