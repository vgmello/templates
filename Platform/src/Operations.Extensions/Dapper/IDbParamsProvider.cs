using Dapper;

namespace Operations.Extensions.Dapper;

public interface IDbParamsProvider
{
    DynamicParameters ToDbParams();
}