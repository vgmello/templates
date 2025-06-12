// Copyright (c) ABCDEG. All rights reserved.

// Added for IEnumerable<T>

using System.Data;
using Dapper;
using Npgsql;

namespace Operations.Extensions.Dapper;

public static class NpgsqlDataSourceExtensions
{
    /// <summary>
    ///     Executes a stored procedure that returns the number of affected rows.
    /// </summary>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="spName">The name of the stored procedure.</param>
    /// <param name="parameters">Provider for command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    public static async Task<int> CallSp(
        this NpgsqlDataSource dataSource,
        string spName,
        IDbParamsProvider parameters,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var dbParams = parameters.ToDbParams();
        var command = new CommandDefinition(
            commandText: spName,
            parameters: dbParams,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command);
    }

    /// <summary>
    ///     Executes a database command that returns the number of affected rows.
    /// </summary>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="commandProvider">Provider for command name and type.</param>
    /// <param name="paramsProvider">Optional provider for command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    public static async Task<int> ExecuteDbCommandAsync(
        this NpgsqlDataSource dataSource,
        IDbCommandProvider commandProvider,
        IDbParamsProvider? paramsProvider = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var commandName = commandProvider.CommandName;
        var commandType = commandProvider.CommandType;
        var dbParams = paramsProvider?.ToDbParams();

        var command = new CommandDefinition(
            commandText: commandName,
            parameters: dbParams,
            commandType: commandType,
            cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command);
    }

    /// <summary>
    ///     Executes a query, returning the data typed as T.
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="commandProvider">Provider for command name and type.</param>
    /// <param name="paramsProvider">Optional provider for command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     A sequence of data of the T type; if a basic type (int, string, etc.) is queried then the data from the first column in assumed,
    ///     otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<IEnumerable<T>> QueryDbCommandAsync<T>(
        this NpgsqlDataSource dataSource,
        IDbCommandProvider commandProvider,
        IDbParamsProvider? paramsProvider = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            commandText: commandProvider.CommandName,
            parameters: paramsProvider?.ToDbParams(),
            commandType: commandProvider.CommandType,
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<T>(command);
    }

    /// <summary>
    ///     Executes a query and returns a single row. Throws an exception if there is not exactly one row.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="commandProvider">Provider for command name and type.</param>
    /// <param name="paramsProvider">Optional provider for command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     A single row of data of the T type; if a basic type (int, string, etc.) is queried then the data from the first column in assumed,
    ///     otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<T> QuerySingleDbCommandAsync<T>(
        this NpgsqlDataSource dataSource,
        IDbCommandProvider commandProvider,
        IDbParamsProvider? paramsProvider = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            commandText: commandProvider.CommandName,
            parameters: paramsProvider?.ToDbParams(),
            commandType: commandProvider.CommandType,
            cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<T>(command);
    }

    /// <summary>
    ///     Executes a query and returns a single row, or a default value if the sequence is empty. Throws an exception if there is more than one
    ///     element.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="commandProvider">Provider for command name and type.</param>
    /// <param name="paramsProvider">Optional provider for command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     A single row of data of the T type; if a basic type (int, string, etc.) is queried then the data from the first column in assumed,
    ///     otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<T?> QuerySingleOrDefaultDbCommandAsync<T>(
        this NpgsqlDataSource dataSource,
        IDbCommandProvider commandProvider,
        IDbParamsProvider? paramsProvider = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            commandText: commandProvider.CommandName,
            parameters: paramsProvider?.ToDbParams(),
            commandType: commandProvider.CommandType,
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    /// <summary>
    ///     Executes a query and returns the first row. Throws an exception if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="commandProvider">Provider for command name and type.</param>
    /// <param name="paramsProvider">Optional provider for command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     The first row of data of the T type; if a basic type (int, string, etc.) is queried then the data from the first column in
    ///     assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<T> QueryFirstDbCommandAsync<T>(
        this NpgsqlDataSource dataSource,
        IDbCommandProvider commandProvider,
        IDbParamsProvider? paramsProvider = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            commandText: commandProvider.CommandName,
            parameters: paramsProvider?.ToDbParams(),
            commandType: commandProvider.CommandType,
            cancellationToken: cancellationToken);

        return await connection.QueryFirstAsync<T>(command);
    }

    /// <summary>
    ///     Executes a query and returns the first row, or a default value if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="commandProvider">Provider for command name and type.</param>
    /// <param name="paramsProvider">Optional provider for command parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     The first row of data of the T type; if a basic type (int, string, etc.) is queried then the data from the first column in
    ///     assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<T?> QueryFirstOrDefaultDbCommandAsync<T>(
        this NpgsqlDataSource dataSource,
        IDbCommandProvider commandProvider,
        IDbParamsProvider? paramsProvider = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            commandText: commandProvider.CommandName,
            parameters: paramsProvider?.ToDbParams(),
            commandType: commandProvider.CommandType,
            cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<T>(command);
    }
}
