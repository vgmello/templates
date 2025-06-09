// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using Microsoft.AspNetCore.Mvc.Testing;
using Grpc.Net.Client;
using Shouldly;

namespace Billing.Tests.Integration;

public class CashierServiceTests(BillingApiWebAppFactory factory) : IClassFixture<BillingApiWebAppFactory>
{
    [Fact]
    public async Task CreateAndGetCashier_ReturnsCashier()
    {
        // Arrange
        var client = CreateClient(factory);
        var name = "Test Cashier";
        var email = "test@example.com";

        // Act - Create cashier
        var createResponse = await client.CreateCashierAsync(new CreateCashierRequest 
        {
            Name = name,
            Email = email
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Act - Get cashier
        var getResponse = await client.GetCashierAsync(new GetCashierRequest 
        {
            Id = createResponse.CashierId
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        createResponse.Name.ShouldBe(name);
        createResponse.Email.ShouldBe(email);
        
        getResponse.CashierId.ShouldBe(createResponse.CashierId);
        getResponse.Name.ShouldBe(name);
        getResponse.Email.ShouldBe(email);
    }

    [Fact]
    public async Task GetCashiers_ReturnsEmptyList_WhenNoCashiers()
    {
        // Arrange
        var client = CreateClient(factory);

        // Act
        var response = await client.GetCashiersAsync(new GetCashiersRequest 
        {
            Limit = 10,
            Offset = 0
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.Cashiers.Count.ShouldBe(0);
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
