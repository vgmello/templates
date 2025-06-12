// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Contracts.Ledgers.Models;
using Accounting.Ledgers.Queries;
using Npgsql;
using Dapper; // Required for QuerySingleOrDefaultAsync extension method signature

namespace Accounting.Tests.Unit.Queries;

public class GetLedgerQueryHandlerTests
{
    private readonly NpgsqlConnection _mockDbConnection;

    public GetLedgerQueryHandlerTests()
    {
        // NSubstitute can mock concrete classes. However, QuerySingleOrDefaultAsync is a static extension method.
        // Similar to ExecuteAsync, true mocking of Dapper's behavior is complex without specific Dapper mocking
        // libraries or refactoring SUT to use interfaces for DB calls.
        // The setup below aims to make the mock return expected values when the handler calls the extension method.
        _mockDbConnection = Substitute.For<NpgsqlConnection>();
    }

    [Fact]
    public async Task Handle_GetLedgerQuery_ShouldReturnLedger_WhenFound()
    {
        // Arrange
        var queryId = Guid.NewGuid();
        var query = new GetLedgerQuery(queryId);

        var expectedLedger = new Ledger
        {
            LedgerId = queryId,
            ClientId = Guid.NewGuid(),
            LedgerType = LedgerType.Cash,
            BalanceDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Setup Dapper's QuerySingleOrDefaultAsync to return the expected ledger
        // This relies on NSubstitute's ability to intercept the call to the extension method,
        // which might be limited. The test verifies that the handler calls this method.
        _mockDbConnection.QuerySingleOrDefaultAsync<Ledger>(
            Arg.Any<string>(), // We can be more specific with Arg.Is if needed
            Arg.Any<object>(),
            null, // transaction
            null, // commandTimeout
            null  // commandType
        ).Returns(Task.FromResult<Ledger?>(expectedLedger));

        // Act
        var result = await GetLedgerQueryHandler.Handle(
            query,
            _mockDbConnection,
            CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedLedger);

        // Verify that QuerySingleOrDefaultAsync was called
        await _mockDbConnection.Received(1).QuerySingleOrDefaultAsync<Ledger>(
            Arg.Is<string>(sql =>
                sql.Contains("SELECT") &&
                sql.Contains("LedgerBalanceId AS LedgerId") &&
                sql.Contains("ClientId") &&
                sql.Contains("LedgerType") &&
                sql.Contains("BalanceDate") &&
                sql.Contains("FROM LedgerBalances") &&
                sql.Contains("WHERE LedgerBalanceId = @Id")
            ),
            Arg.Is<object>(param => param.GetType().GetProperty("Id") != null && (Guid)param.GetType().GetProperty("Id").GetValue(param, null) == queryId),
            null, // transaction
            null, // commandTimeout
            CommandType.Text // Assuming default command type
        );
    }

    [Fact]
    public async Task Handle_GetLedgerQuery_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var queryId = Guid.NewGuid();
        var query = new GetLedgerQuery(queryId);

        // Setup Dapper's QuerySingleOrDefaultAsync to return null
        _mockDbConnection.QuerySingleOrDefaultAsync<Ledger>(
            Arg.Any<string>(),
            Arg.Any<object>(),
            null, null, null
        ).Returns(Task.FromResult<Ledger?>(null));

        // Act
        var result = await GetLedgerQueryHandler.Handle(
            query,
            _mockDbConnection,
            CancellationToken.None);

        // Assert
        result.ShouldBeNull();

        // Verify that QuerySingleOrDefaultAsync was called
        await _mockDbConnection.Received(1).QuerySingleOrDefaultAsync<Ledger>(
            Arg.Is<string>(sql => sql.Contains("WHERE LedgerBalanceId = @Id")),
            Arg.Is<object>(param => param.GetType().GetProperty("Id") != null && (Guid)param.GetType().GetProperty("Id").GetValue(param, null) == queryId),
            null,
            null,
            CommandType.Text
        );
    }
}
