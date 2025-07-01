---
title: DbDataSource extensions for Dapper operations
description: Learn about the DbDataSource extension methods that simplify executing stored procedures and queries using Dapper.
---

# DbDataSource extensions for Dapper operations

The `DbDataSourceExtensions` class provides a set of extension methods for `DbDataSource` objects, streamlining the execution of stored procedures and queries with Dapper. These extensions abstract away common boilerplate code, making it easier to interact with your database.

## Understanding DbDataSourceExtensions

This class offers the following key methods:

*   **`SpExecute`**: Executes a stored procedure that is expected to return the number of affected rows (e.g., for `INSERT`, `UPDATE`, `DELETE` operations).
*   **`SpQuery<TResult>`**: Executes a stored procedure that is expected to return a collection of results of type `TResult` (e.g., for `SELECT` operations).
*   **`SpCall<TResult>`**: A generic internal method that serves as the core logic for both `SpExecute` and `SpQuery`. It handles opening the database connection, preparing the command definition, and executing the Dapper function.

All these methods leverage the `IDbParamsProvider` interface to convert command or query objects into database parameters, ensuring a consistent approach to parameter handling.

## Usage examples

### Executing a stored procedure with `SpExecute`

Use `SpExecute` when your stored procedure performs an action and returns the number of rows affected.

```csharp
using System.Data.Common;
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Dapper;

// Define a command that implements IDbParamsProvider (source-generated via DbCommandAttribute)
[DbCommand(sp: "dbo.UpdateUserStatus", nonQuery: true)]
public partial record UpdateUserStatusCommand(int UserId, bool IsActive) : ICommand<int>;

public class ExampleService
{
    private readonly DbDataSource _dataSource;

    public ExampleService(DbDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task UpdateUser(int userId, bool isActive)
    {
        var command = new UpdateUserStatusCommand(userId, isActive);
        int affectedRows = await _dataSource.SpExecute("dbo.UpdateUserStatus", command, CancellationToken.None);
        Console.WriteLine($"Updated {affectedRows} rows for user {userId}.");
    }
}
```

### Querying data with `SpQuery<TResult>`

Use `SpQuery<TResult>` when your stored procedure retrieves data and returns a collection of objects.

```csharp
using System.Collections.Generic;
using System.Data.Common;
using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Dapper;

// Define a query that implements IDbParamsProvider (source-generated via DbCommandAttribute)
[DbCommand(sp: "dbo.GetActiveUsers")]
public partial record GetActiveUsersQuery() : IQuery<IEnumerable<User>>;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
}

public class UserService
{
    private readonly DbDataSource _dataSource;

    public UserService(DbDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IEnumerable<User>> GetActiveUsers()
    {
        var query = new GetActiveUsersQuery();
        IEnumerable<User> activeUsers = await _dataSource.SpQuery<User>("dbo.GetActiveUsers", query, CancellationToken.None);
        foreach (var user in activeUsers)
        {
            Console.WriteLine($"Active User: {user.Name} (ID: {user.Id})");
        }
        return activeUsers;
    }
}
```

## See also

*   [Dapper](https://github.com/DapperLib/Dapper)
*   [IDbParamsProvider](idbparams-provider.md)
