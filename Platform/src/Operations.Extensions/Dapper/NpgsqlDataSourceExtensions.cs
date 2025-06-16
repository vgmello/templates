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
}
