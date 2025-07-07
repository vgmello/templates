// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Messaging;

/// <summary>
///     Represents a query that can be sent through the message bus to retrieve data.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
/// <remarks>
///     This is a marker interface used by the messaging infrastructure to identify query messages.
///     Queries are read-only operations that retrieve data without modifying the system state.
/// </remarks>
public interface IQuery<out TResult>
{
    /// <summary>
    ///     Gets an empty result for this query type.
    /// </summary>
    /// <value>
    ///     The default value for the result type, typically <c>null</c> for reference types
    ///     or the default value for value types.
    /// </value>
    TResult? Empty => default;
}
