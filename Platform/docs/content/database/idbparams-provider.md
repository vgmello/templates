---
title: IDbParamsProvider for database parameter conversion
description: Learn about the IDbParamsProvider interface, which defines a contract for objects that can convert their properties into a format suitable for database parameters.
---

# IDbParamsProvider for database parameter conversion

The `IDbParamsProvider` interface plays a crucial role in standardizing how objects expose their data for use as database parameters. It defines a single method, `ToDbParams()`, which is responsible for transforming the object's properties into a format that can be consumed by database command execution libraries like Dapper.

## Understanding IDbParamsProvider

This interface acts as a contract, ensuring that any class implementing it can provide a representation of its data specifically tailored for database operations. While you typically won't implement this interface manually when using the `DbCommandAttribute` (as the source generator handles it), understanding its purpose is key to comprehending the underlying mechanism of database parameter generation.

## Usage context

When you apply the `DbCommandAttribute` to a class, the associated source generator automatically implements the `IDbParamsProvider` interface for that class. The generated `ToDbParams()` method then handles the conversion of your class's properties into an anonymous object or a `DynamicParameters` object (for Dapper) that contains the correctly named and cased database parameters.

This abstraction allows the command execution logic to uniformly retrieve parameters from any object that represents a database command or query, regardless of its internal structure.

## Example (conceptual)

While you don't typically write this code yourself, here's a conceptual idea of what an implementation might look like:

```csharp
using Operations.Extensions.Abstractions.Dapper;
using Dapper;

// This implementation is conceptually what the source generator would produce
public class MyCommand : IDbParamsProvider
{
    public string Name { get; set; }
    public int Age { get; set; }

    public object ToDbParams()
    {
        // In a real scenario, the source generator would handle casing (e.g., snake_case)
        // and other complexities based on DbCommandAttribute settings.
        return new { Name, Age };
    }
}

// Usage by a command handler (also typically source-generated)
public class CommandHandler
{
    public void Execute(MyCommand command)
    {
        var dbParams = command.ToDbParams();
        // Use dbParams with Dapper to execute a query or command
        // For example: connection.Execute("INSERT INTO MyTable VALUES (@Name, @Age)", dbParams);
    }
}
```

## See also

*   [DbCommandAttribute](dbcommand-attribute.md)
*   [Dapper](https://github.com/DapperLib/Dapper)
