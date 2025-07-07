---
title: ColumnAttribute for custom parameter naming
description: Learn how to use the ColumnAttribute to specify a custom database parameter name for individual properties, overriding default naming conventions.
---

# ColumnAttribute for custom parameter naming

The `ColumnAttribute` allows you to explicitly define the database parameter name for a property within a class that is used with `DbCommandAttribute`. This is particularly useful when your C# property names do not directly match the column names in your database, or when you need to override the default naming conventions (like `SnakeCase`) specified by `DbCommandAttribute`.

## Understanding ColumnAttribute

When applied to a parameter in a record or class constructor, the `ColumnAttribute` ensures that the corresponding property's value is mapped to the specified database parameter name.

## Usage example

Consider a scenario where your database column is named `user_id`, but your C# property is `UserId`. You can use `ColumnAttribute` to bridge this naming difference:

```csharp
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

[DbCommand(sp: "dbo.GetUserById")]
public record GetUserQuery([Column("user_id")] int UserId) : IQuery<User>;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// When GetUserQuery is used, the UserId property will be mapped to a database parameter named 'user_id'.
```

In this example, even if `DbCommandAttribute` had a `paramsCase` set to `None` or `SnakeCase`, the `[Column("user_id")]` on the `UserId` parameter would ensure that the parameter sent to the `dbo.GetUserById` stored procedure is precisely `user_id`.

## See also

*   [DbCommandAttribute](dbcommand-attribute.md)
