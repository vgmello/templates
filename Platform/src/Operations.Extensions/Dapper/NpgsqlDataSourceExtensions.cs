using System.Data;
using Dapper; // Ensure Dapper namespace is used for DynamicParameters
using Npgsql;

namespace Operations.Extensions.Dapper;

public static class NpgsqlDataSourceExtensions
{
    public static async Task<int> CallSp(this NpgsqlDataSource dataSource, string spName, DynamicParameters dbParams, // No need to qualify Dapper.DynamicParameters if using Dapper; is present
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        // The 'parameters' argument is now already Dapper.DynamicParameters
        // No need for: var dbParams = parameters.ToDbParams();

        return await connection.ExecuteAsync(spName, dbParams, commandType: CommandType.StoredProcedure);
    }
}
