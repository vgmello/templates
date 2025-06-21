// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Queries;
using Billing.Tests.Integration;
using Shouldly;

namespace Billing.Tests.Unit;

public class CashierQueryHandlersTests
{
    [Fact]
    public async Task GetCashiers_ReturnsEmptyList_WhenNoCashiers()
    {
        // Arrange
        var query = new GetCashiersQuery { Limit = 10, Offset = 0 };

        // Act
        var result = await GetCashiersQueryHandler.Handle(query, _fixture.DataSource, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }
}
