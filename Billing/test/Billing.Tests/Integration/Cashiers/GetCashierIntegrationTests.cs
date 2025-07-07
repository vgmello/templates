// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Grpc;
using Billing.Tests.Integration._Internal;
using Grpc.Core;

namespace Billing.Tests.Integration.Cashiers;

public class GetCashierIntegrationTests(IntegrationTestFixture fixture) : IntegrationTest(fixture)
{
    private readonly CashiersService.CashiersServiceClient _client = new(fixture.GrpcChannel);

    [Fact]
    public async Task GetCashier_WithValidId_ShouldReturnCashier()
    {
        // Arrange - First create a cashier
        var createRequest = new CreateCashierRequest
        {
            Name = "Test Cashier for Get",
            Email = "get@test.com"
        };

        var createResponse = await _client.CreateCashierAsync(createRequest, cancellationToken: TestContext.Current.CancellationToken);

        var getRequest = new GetCashierRequest { Id = createResponse.CashierId };

        // Act
        var response = await _client.GetCashierAsync(getRequest, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldBe(new Billing.Cashiers.Grpc.Models.Cashier
        {
            CashierId = createResponse.CashierId,
            Name = createRequest.Name,
            Email = createRequest.Email
        });
    }

    [Fact]
    public async Task GetCashier_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var request = new GetCashierRequest { Id = Guid.NewGuid().ToString() };

        // Act & Assert
        await Should.ThrowAsync<RpcException>(async () =>
            await _client.GetCashierAsync(request, cancellationToken: TestContext.Current.CancellationToken));
    }
}
