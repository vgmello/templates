// Copyright (c) ABCDEG. All rights reserved.

using System;

namespace Operations.Extensions.Dapper;

/// <summary>
/// Attribute to define database command properties and behavior for a class.
/// Triggers generation of a ToDbParams() method. If 'sp' or 'sql' is provided,
/// also triggers generation of a command handler method.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DbCommandAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the stored procedure.
    /// If set, a command handler will be generated using this stored procedure.
    /// </summary>
    public string? Sp { get; }

    /// <summary>
    /// Gets the SQL query text.
    /// If set, a command handler will be generated using this SQL query.
    /// </summary>
    public string? Sql { get; }

    /// <summary>
    /// Gets a value indicating whether property names should be converted to snake_case
    /// for DB parameter names in the generated ToDbParams() method.
    /// This is overridden by [System.ComponentModel.DataAnnotations.Schema.Column] attribute on a property.
    /// Default is false (uses property names as-is).
    /// </summary>
    public bool UseSnakeCase { get; }

    /// <summary>
    /// For commands implementing ICommand&lt;int&gt;, indicates whether the returned integer
    /// represents the number of affected records (true, default) or a scalar integer value returned by the query (false).
    /// This property may be effectively overridden if NonQuery is set to true.
    /// </summary>
    public bool ReturnsAffectedRecords { get; }

    /// <summary>
    /// Gets a value indicating that the command is primarily non-query, even if it implements ICommand&lt;int&gt;.
    /// If true for ICommand&lt;int&gt;, the handler will use ExecuteAsync and return rows affected, overriding ReturnsAffectedRecords behavior.
    /// If true for ICommand&lt;TResult&gt; where TResult is not int, this may lead to diagnostics or unexpected behavior as query results are expected.
    /// Default is false.
    /// </summary>
    public bool NonQuery { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbCommandAttribute"/> class.
    /// </summary>
    /// <param name="sp">The name of the stored procedure. Mutually exclusive with <paramref name="sql"/>.</param>
    /// <param name="sql">The SQL query text. Mutually exclusive with <paramref name="sp"/>.</param>
    /// <param name="useSnakeCase">If true, maps command class property names to snake_case for DB parameters in the generated ToDbParams() method. Default is false.</param>
    /// <param name="returnsAffectedRecords">For commands implementing ICommand&lt;int&gt;, specifies if the integer result represents affected records (default is true) or a scalar query result (false).</param>
    /// <param name="nonQuery">Indicates if the command is primarily a non-query operation. If true, this may influence handler generation, particularly for ICommand&lt;int&gt; to ensure ExecuteAsync is used. Default is false.</param>
    public DbCommandAttribute(
        string? sp = null,
        string? sql = null,
        bool useSnakeCase = false,
        bool returnsAffectedRecords = true,
        bool nonQuery = false) // Added nonQuery parameter
    {
        if (!string.IsNullOrWhiteSpace(sp) && !string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("Cannot provide both 'sp' and 'sql' parameters. Choose one, or neither if only configuring ToDbParams behavior.");
        }

        Sp = sp;
        Sql = sql;
        UseSnakeCase = useSnakeCase;
        ReturnsAffectedRecords = returnsAffectedRecords;
        NonQuery = nonQuery; // Set the NonQuery property
    }
}
