---
title: DbCommandAttribute for database command generation
description: Learn how to use the DbCommandAttribute to define database command properties and trigger source generation for Dapper-based command handlers.
---

# DbCommandAttribute for database command generation

The `DbCommandAttribute` is a powerful tool that allows you to define database command properties directly on your C# classes. When applied, it triggers a source generator to automatically create a `ToDbParams()` method for your class and, optionally, a command handler method for executing database operations using Dapper.

This attribute simplifies the process of interacting with databases by reducing boilerplate code for mapping object properties to database parameters and executing commands.

## Understanding DbCommandAttribute

The `DbCommandAttribute` can be applied to classes and provides several parameters to configure the generated code:

*   **`sp` (Stored Procedure)**: Specifies the name of a stored procedure to execute. Mutually exclusive with `sql` and `fn`.
*   **`sql` (SQL Query)**: Provides a raw SQL query string to execute. Mutually exclusive with `sp` and `fn`.
*   **`fn` (Function SQL Query)**: Specifies a function SQL query. Parameters are automatically generated based on the record properties. Mutually exclusive with `sp` and `sql`.
*   **`paramsCase` (DbParamsCase)**: Defines how property names are converted to database parameter names (e.g., `SnakeCase` for `snake_case`).
*   **`nonQuery`**: Indicates if the command is a non-query operation (e.g., `INSERT`, `UPDATE`, `DELETE`). This primarily affects how `ICommand<int>` is handled.
*   **`dataSource`**: Specifies a keyed data source to use for the command.

## DbParamsCase enumeration

The `DbParamsCase` enumeration controls the naming convention for database parameters generated from your class properties:

*   **`Unset`**: Uses the global default specified by the `DbCommandDefaultParamCase` MSBuild property.
*   **`None`**: Uses property names as-is without any conversion.
*   **`SnakeCase`**: Converts property names to `snake_case` (e.g., `FirstName` becomes `first_name`).

Individual properties can override this behavior using the `[Column("custom_name")]` attribute.

## Usage examples

### Executing a stored procedure

To execute a stored procedure, use the `sp` parameter:

```csharp
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

[DbCommand(sp: "dbo.CreateUser", nonQuery: true)]
public record CreateUserCommand(string UserName, string Email) : ICommand<int>;

// The source generator will create a handler that executes 'dbo.CreateUser'
// and maps UserName and Email to parameters.
```

### Executing a raw SQL query

For direct SQL execution, use the `sql` parameter:

```csharp
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

[DbCommand(sql: "INSERT INTO Users (UserName, Email) VALUES (@UserName, @Email)", nonQuery: true)]
public record InsertUserCommand(string UserName, string Email) : ICommand<int>;

// The source generator will create a handler that executes the provided SQL query.
```

### Executing a function SQL query

When calling a database function, use the `fn` parameter. The parameters will be automatically appended based on record properties.

```csharp
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

[DbCommand(fn: "SELECT dbo.GetUserCount(@MinAge)")]
public record GetUserCountQuery(int MinAge) : IQuery<int>;

// The source generator will create a handler that executes the function
// and passes MinAge as a parameter.
```

### Controlling parameter naming with DbParamsCase

You can specify the casing for generated database parameters using `paramsCase`:

```csharp
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

[DbCommand(sp: "dbo.UpdateProduct", paramsCase: DbParamsCase.SnakeCase, nonQuery: true)]
public record UpdateProductCommand(int ProductId, string ProductName, decimal UnitPrice) : ICommand<int>;

// Properties like ProductId, ProductName, UnitPrice will be converted to product_id, product_name, unit_price
// when passed as database parameters.
```

### Specifying a data source

If you have multiple data sources configured, you can specify which one to use:

```csharp
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

[DbCommand(sp: "dbo.GetOrderDetails", dataSource: "OrderDb")]
public record GetOrderDetailsQuery(int OrderId) : IQuery<OrderDetails>;

// This command will use the data source registered with the key "OrderDb".
```

## See also

*   [Dapper](https://github.com/DapperLib/Dapper)
*   [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
