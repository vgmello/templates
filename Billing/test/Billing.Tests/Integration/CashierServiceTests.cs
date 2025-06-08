// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using Microsoft.AspNetCore.Mvc.Testing;
using Grpc.Net.Client;
using Shouldly;

namespace Billing.Tests.Integration;

public class CashierServiceTests(BillingApiWebAppFactory factory) : IClassFixture<BillingApiWebAppFactory>
{
    [Fact]
    public async Task GetCashier_ReturnsCashier()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var client = CreateClient(factory);

        // Act
        var response = await client.GetCashierAsync(new GetCashierRequest { Id = expectedId.ToString() },
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.CashierId.ShouldBe(expectedId.ToString());
    }

    private static CashiersService.CashiersServiceClient CreateClient(WebApplicationFactory<Program> factory)
    {
        var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = factory.Server.CreateHandler()
        });

        return new CashiersService.CashiersServiceClient(channel);
    }
}
