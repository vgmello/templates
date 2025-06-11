// Copyright (c) ABCDEG. All rights reserved.

using System;
using Operations.Extensions.Dapper; // For DbCommandAttribute
using Operations.Extensions.Messaging; // For ICommand<int>

namespace Billing.Cashier.Commands;

[DbCommand(sp: "billing.create_cashier", UseSnakeCase = true, ReturnsAffectedRecords = true)]
public record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;
// Properties CashierId, Name, Email are public by virtue of being a record.
// UseSnakeCase = true will map: CashierId -> cashier_id, Name -> name, Email -> email for ToDbParams.
// ReturnsAffectedRecords = true means the ICommand<int> result is from ExecuteAsync (rows affected).
