// Copyright (c) ABCDEG. All rights reserved.

using System.Data;

namespace Operations.Extensions.Dapper;

/// <summary>
/// Interface for types that provide database command information.
/// </summary>
public interface IDbCommandProvider
{
    /// <summary>
    /// Gets the name of the stored procedure or the SQL query string.
    /// </summary>
    string CommandName { get; }

    /// <summary>
    /// Gets the type of the command.
    /// </summary>
    CommandType CommandType { get; }
}
