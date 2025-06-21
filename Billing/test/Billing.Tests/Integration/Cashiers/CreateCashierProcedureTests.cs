// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using Billing.Tests.Integration._Internal;

namespace Billing.Tests.Integration.Cashiers;

public class CreateCashierIntegrationTests(IntegrationTestFixture fixture) : IntegrationTest
{
    private readonly CashiersService.CashiersServiceClient _client = new(fixture.GrpcChannel);

    [Fact]
    public async Task CreateCashier_ShouldCreateCashierInDatabase()
    {
        // Arrange
        var request = new CreateCashierRequest
        {
            Name = "Integration Test Cashier",
            Email = "integration@test.com"
        };

        // Act
        var response = await _client.CreateCashierAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        response.Name.ShouldBe("Integration Test Cashier");
        response.Email.ShouldBe("integration@test.com");
        Guid.Parse(response.CashierId).ShouldNotBe(Guid.Empty);
    }
}
