// Copyright (c) ABCDEG. All rights reserved.

using System;
using Operations.Extensions.Dapper; // For DbCommandAttribute
using Operations.Extensions.Messaging; // For ICommand<int>

namespace Billing.Cashier.Commands;

[DbCommand(sp: "billing.create_cashier", UseSnakeCase = true, ReturnsAffectedRecords = true, NonQuery = true)]
public record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;
// Note: As per generator logic, if NonQuery = true, ReturnsAffectedRecords is effectively ignored for ICommand<int>
// and ExecuteAsync (rows affected) is used. Setting ReturnsAffectedRecords = true here is fine for clarity if NonQuery was false.
