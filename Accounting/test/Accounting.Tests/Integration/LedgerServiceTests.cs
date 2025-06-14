// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Ledgers.Grpc;
using Microsoft.AspNetCore.Mvc.Testing;
using Grpc.Net.Client;
using Shouldly;

namespace Accounting.Tests.Integration;

public class LedgerServiceTests(AccountingApiWebAppFactory factory) : IClassFixture<AccountingApiWebAppFactory>
{
    [Fact]
    public async Task GetLedger_ReturnsLedger()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var client = CreateClient(factory);

        // Act
        var response = await client.GetLedgerAsync(new GetLedgerRequest { Id = expectedId.ToString() },
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.LedgerId.ShouldBe(expectedId.ToString());
    }

    private static LedgersService.LedgersServiceClient CreateClient(WebApplicationFactory<Program> factory)
    {
        var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = factory.Server.CreateHandler()
        });

        return new LedgersService.LedgersServiceClient(channel);
    }
}
