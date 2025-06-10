using System.Data;
using Dapper;
using Npgsql;

namespace Operations.Extensions.Dapper;

public static class NpgsqlDataSourceExtensions
{
    public static async Task<int> CallSp(this NpgsqlDataSource dataSource, string spName, IDbParamsProvider parameters,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var dbParams = parameters.ToDbParams();

        return await connection.ExecuteAsync(spName, dbParams, commandType: CommandType.StoredProcedure);
    }
}
