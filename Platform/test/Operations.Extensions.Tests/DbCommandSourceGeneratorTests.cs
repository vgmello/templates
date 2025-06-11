using Npgsql;
using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Operations.Extensions.Tests;

public static partial class SampleHandler
{
    [DbParams]
    [DbCommand("sample.create_item")]
    public partial record SampleInsertCommand(string Name) : ICommand<int>;
}

public class DbCommandSourceGeneratorTests
{
    [Fact]
    public void HandleMethod_Generated()
    {
        var method = typeof(SampleHandler).GetMethod(
            "Handle",
            new[] { typeof(SampleHandler.SampleInsertCommand), typeof(NpgsqlDataSource), typeof(CancellationToken) });

        method.ShouldNotBeNull();
    }
}
