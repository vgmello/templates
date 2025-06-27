---
title: IQuery<TResult> for query messaging
description: Learn about the IQuery<TResult> interface, a marker interface for defining queries that produce a result of a specific type.
---

# IQuery<TResult> for query messaging

The `IQuery<TResult>` interface serves as a marker interface within the messaging system, specifically for queries. It signifies that a class represents a query that, upon execution, is expected to retrieve and return data of type `TResult`.

This interface is crucial for distinguishing read-only operations (queries) from write operations (commands) within a Command Query Responsibility Segregation (CQRS) architecture. It promotes a clear separation of concerns and helps in building more maintainable and scalable applications.

## Understanding IQuery<TResult>

*   **Marker Interface**: Like `ICommand<TResult>`, `IQuery<TResult>` does not define any methods for you to implement. Its primary role is to identify and categorize query objects that are designed to fetch data.
*   **`TResult`**: The `out` keyword on `TResult` indicates covariance, allowing for flexible handling of query results where a more derived type can be treated as a base type. This is beneficial for polymorphism in query handling.
*   **`Empty` Property**: The `Empty` property provides a default value for `TResult`. This can be useful for scenarios where a query might not return any data, or for initializing a default state.

## Usage example

You typically implement `IQuery<TResult>` on a record or class that encapsulates the criteria needed to perform a data retrieval operation. The actual logic for executing the query and fetching data is usually separated into a query handler.

```csharp
using Operations.Extensions.Abstractions.Messaging;

// Define a query to get a user by their ID, expecting a User object as a result
public record GetUserByIdQuery(int UserId) : IQuery<User>;

// Define a query to get a list of all active products, expecting a list of Product objects
public record GetAllActiveProductsQuery() : IQuery<List<Product>>;

// Define a query to check if an email exists, expecting a boolean result
public record CheckEmailExistsQuery(string Email) : IQuery<bool>;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

// In your application, you would then have handlers that process these queries:
public class GetUserByIdQueryHandler // (Conceptual - not part of IQuery itself)
{
    public User Handle(GetUserByIdQuery query)
    {
        // Logic to retrieve user from database
        Console.WriteLine($"Retrieving user with ID: {query.UserId}");
        return new User { Id = query.UserId, Name = "John Doe", Email = "john.doe@example.com" };
    }
}
```

## See also

*   [ICommand<TResult>](icommand.md)
