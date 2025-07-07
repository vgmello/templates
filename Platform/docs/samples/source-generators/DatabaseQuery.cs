using System.Data.Common;
using Operations.Extensions.Abstractions.Dapper;

namespace Operations.Samples.SourceGenerators;

[DbCommand("SELECT * FROM a_table WHERE id = @Id")]
public partial class DatabaseQuery : IDbParamsProvider<DatabaseQuery.DbParams>
{
    public record DbParams(int Id);
}
