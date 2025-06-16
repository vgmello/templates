// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Dapper;

/// <summary>
///     Attribute to define database command properties and behavior for a class.
///     Triggers generation of a ToDbParams() method. If 'sp' or 'sql' is provided,
///     also triggers generation of a command handler method.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DbCommandAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbCommandAttribute" /> class.
    /// </summary>
    /// <param name="sp">The name of the stored procedure. Mutually exclusive with <paramref name="sql" />.</param>
    /// <param name="sql">The SQL query text. Mutually exclusive with <paramref name="sp" />.</param>
    /// <param name="paramsCase">Indicates whether property names should be converted to snake_case</param>
    /// <param name="nonQuery">
    ///     Indicates the nature of the command, with its primary effect on commands implementing ICommand&lt;int&gt;.
    ///     Default is true.
    ///     If true: For ICommand&lt;int&gt;, the generated handler uses ExecuteAsync (rows affected). For other ICommand&lt;TResult&gt;, a
    ///     diagnostic may be issued.
    ///     If false: For ICommand&lt;int&gt;, the generated handler uses ExecuteScalarAsync&lt;int&gt;. For other ICommand&lt;TResult&gt;, a query
    ///     is performed.
    /// </param>
    public DbCommandAttribute(
        string? sp = null,
        string? sql = null,
        DbParamsCase paramsCase = DbParamsCase.Default,
        bool nonQuery = false)
    {
        if (!string.IsNullOrWhiteSpace(sp) && !string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException(
                "Cannot provide both 'sp' and 'sql' parameters. Choose one, or neither if only configuring ToDbParams behavior.");
        }

        Sp = sp;
        Sql = sql;
        ParamsCase = paramsCase;
        NonQuery = nonQuery;
    }

    /// <summary>
    ///     Gets the name of the stored procedure.
    ///     If set, a command handler will be generated using this stored procedure.
    /// </summary>
    public string? Sp { get; }

    /// <summary>
    ///     Gets the SQL query text.
    ///     If set, a command handler will be generated using this SQL query.
    /// </summary>
    public string? Sql { get; }

    /// <summary>
    ///     Gets a value indicating whether property names should be converted to snake_case
    ///     for DB parameter names in the generated ToDbParams() method.
    ///     This is overridden by [System.ComponentModel.DataAnnotations.Schema.Column] attribute on a property.
    ///     Default is uses property names as-is.
    /// </summary>
    public DbParamsCase ParamsCase { get; }

    /// <summary>
    ///     Gets a value indicating the nature of the command. This flag primarily influences behavior for ICommand&lt;int&gt;.
    ///     If true:
    ///     - For ICommand&lt;int&gt;: The generated handler will use Dapper's ExecuteAsync (expecting rows affected).
    ///     - For ICommand&lt;TResult&gt; where TResult is not int: A diagnostic warning may be issued by the source generator,
    ///     as using NonQuery=true with a command expecting a specific data structure is atypical. The handler may default
    ///     to an ExecuteAsync call and return default(TResult).
    ///     If false:
    ///     - For ICommand&lt;int&gt;: The generated handler will use Dapper's ExecuteScalarAsync&lt;int&gt; (expecting a scalar integer query
    ///     result).
    ///     - For ICommand&lt;TResult&gt; where TResult is not int: The handler will perform a query (e.g., QueryFirstOrDefaultAsync or
    ///     QueryAsync).
    /// </summary>
    public bool NonQuery { get; }
}

public enum DbParamsCase
{
    /// <summary>
    ///     Use the property names as-is (default).
    /// </summary>
    Default,

    /// <summary>
    ///     Convert property names to snake_case.
    /// </summary>
    SnakeCase
}
