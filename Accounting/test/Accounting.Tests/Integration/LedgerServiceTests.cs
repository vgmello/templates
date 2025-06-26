// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Ledgers.Grpc;
using Accounting.Tests.Integration._Internal;

namespace Accounting.Tests.Integration;

public class LedgerServiceTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
{
    private readonly LedgersService.LedgersServiceClient _client = new(fixture.GrpcChannel);

    [Fact]
    public async Task GetLedger_ReturnsLedger()
    {
        // Arrange
        var expectedId = Guid.CreateVersion7();

        // Act
        var response = await _client.GetLedgerAsync(new GetLedgerRequest { Id = expectedId.ToString() },
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.LedgerId.ShouldBe(expectedId.ToString());
    }
}
