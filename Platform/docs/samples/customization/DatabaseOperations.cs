using System.Data.Common;
using Operations.Extensions.Abstractions.Dapper;

namespace Operations.Samples.Customization;

[DbCommand("INSERT INTO a_table (name) VALUES (@Name)")]
public partial class InsertOperation : IDbParamsProvider<InsertOperation.DbParams>
{
    public record DbParams(string Name);
}
