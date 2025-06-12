// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Contracts.Ledgers.Models;
using Accounting.Ledgers.Queries;
using Npgsql;
using Dapper; // Required for QueryAsync extension method signature

namespace Accounting.Tests.Unit.Queries;

public class GetLedgersQueryHandlerTests
{
    private readonly NpgsqlConnection _mockDbConnection;

    public GetLedgersQueryHandlerTests()
    {
        _mockDbConnection = Substitute.For<NpgsqlConnection>();
    }

    [Fact]
    public async Task Handle_GetLedgersQuery_ShouldReturnListOfLedgers()
    {
        // Arrange
        var query = new GetLedgersQuery { Limit = 10, Offset = 0 };
        var expectedLedgers = new List<GetLedgersQuery.Result>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), LedgerType.Cash),
            new(Guid.NewGuid(), Guid.NewGuid(), LedgerType.AccountsPayable)
        };

        _mockDbConnection.QueryAsync<GetLedgersQuery.Result>(
            Arg.Any<string>(),
            Arg.Any<object>(),
            null, null, null
        ).Returns(Task.FromResult<IEnumerable<GetLedgersQuery.Result>>(expectedLedgers));

        // Act
        var result = await GetLedgersQueryHandler.Handle(
            query,
            _mockDbConnection,
            CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(expectedLedgers.Count);
        result.ShouldBe(expectedLedgers); // Shouldly compares collections element by element

        await _mockDbConnection.Received(1).QueryAsync<GetLedgersQuery.Result>(
            Arg.Is<string>(sql =>
                sql.Contains("SELECT") &&
                sql.Contains("LedgerBalanceId AS LedgerId") &&
                sql.Contains("ClientId") &&
                sql.Contains("LedgerType") &&
                sql.Contains("FROM LedgerBalances") &&
                sql.Contains("ORDER BY LedgerBalanceId") &&
                sql.Contains("LIMIT @Limit OFFSET @Offset")
            ),
            Arg.Is<object>(param =>
                param.GetType().GetProperty("Limit").GetValue(param, null).Equals(query.Limit) &&
                param.GetType().GetProperty("Offset").GetValue(param, null).Equals(query.Offset)
            ),
            null, // transaction
            null, // commandTimeout
            CommandType.Text // Assuming default command type
        );
    }

    [Fact]
    public async Task Handle_GetLedgersQuery_ShouldReturnEmptyList_WhenNoLedgersFound()
    {
        // Arrange
        var query = new GetLedgersQuery { Limit = 10, Offset = 0 };
        var emptyList = new List<GetLedgersQuery.Result>();

        _mockDbConnection.QueryAsync<GetLedgersQuery.Result>(
            Arg.Any<string>(),
            Arg.Any<object>(),
            null, null, null
        ).Returns(Task.FromResult<IEnumerable<GetLedgersQuery.Result>>(emptyList));

        // Act
        var result = await GetLedgersQueryHandler.Handle(
            query,
            _mockDbConnection,
            CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        await _mockDbConnection.Received(1).QueryAsync<GetLedgersQuery.Result>(
            Arg.Is<string>(sql =>
                sql.Contains("LIMIT @Limit OFFSET @Offset")
            ),
            Arg.Is<object>(param =>
                param.GetType().GetProperty("Limit").GetValue(param, null).Equals(query.Limit) &&
                param.GetType().GetProperty("Offset").GetValue(param, null).Equals(query.Offset)
            ),
            null,
            null,
            CommandType.Text
        );
    }
}
