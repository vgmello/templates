using Dapper;

namespace Operations.Extensions.Dapper;

public static class SnakeCaseMappingExtensions
{
    public static void UseSnakeCaseMapping()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}
