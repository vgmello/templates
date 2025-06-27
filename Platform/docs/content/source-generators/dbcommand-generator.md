---
title: DbCommand Generator
description: Details on the DbCommandSourceGenerator, which automates Dapper command and query handler generation.
---

# DbCommand Generator

The `DbCommandSourceGenerator` is a key component within the Platform that leverages C# Source Generators to automate the creation of Dapper-based command and query handlers. This generator significantly reduces the boilerplate code typically associated with database interactions, allowing developers to focus on defining their data transfer objects (DTOs) and commands/queries.

## How it Works

When you apply the `[DbCommand]` attribute to a class or record, the `DbCommandSourceGenerator` analyzes its properties and the parameters defined in the attribute. Based on this information, it generates:

1.  **`ToDbParams()` method**: This method converts the properties of your class into an anonymous object or a `DynamicParameters` object (for Dapper) that can be directly used as parameters for database commands. It handles casing conventions (e.g., snake_case) as specified by the `DbParamsCase` enum or `[Column]` attribute.
2.  **Command/Query Handler**: If you specify `sp`, `sql`, or `fn` in the `[DbCommand]` attribute, the generator also creates a handler method that executes the specified stored procedure, SQL query, or function using Dapper. This handler integrates seamlessly with messaging systems like Wolverine if configured.

## Usage with DbCommandAttribute

The `DbCommandGenerator` works in conjunction with the `[DbCommand]` attribute (defined in `Operations.Extensions.Abstractions.Dapper`). The parameters you provide to this attribute guide the generator in creating the appropriate code.

For detailed usage of the `[DbCommand]` attribute, refer to its documentation.

## Example (Conceptual Generated Code)

Consider a command defined as:

[!code-csharp[](~/docs/samples/dbcommand-generator/CreateUserCommand.cs)]

The `DbCommandSourceGenerator` would conceptually generate code similar to this (simplified for illustration):

[!code-csharp[](~/docs/samples/dbcommand-generator/GeneratedCreateUserCommand.cs)]

This generated code handles the mapping and execution, freeing you from writing repetitive ADO.NET or Dapper calls manually.

## See also

*   [DbCommandAttribute](../database/dbcommand-attribute.md)
*   [Source Generators Overview](overview.md)
*   [Dapper](https://github.com/DapperLib/Dapper)
