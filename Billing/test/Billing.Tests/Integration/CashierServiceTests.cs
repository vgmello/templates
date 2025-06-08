// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using Billing.Cashier.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Grpc.Net.Client;
using Wolverine;
using Shouldly;
using Operations.ServiceDefaults;

namespace Billing.Tests.Integration;

public class CashierServiceTests
{
    [Fact]
    public async Task GetCashier_ReturnsCashier()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ServiceBus__ConnectionString", string.Empty);
        var expectedId = Guid.NewGuid();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseEntryAssembly<Program>();
                builder.ConfigureAppConfiguration((_, cfg) =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ServiceBus:ConnectionString"] = string.Empty
                    });
                });
            });

        var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = factory.Server.CreateHandler()
        });
        var client = new CashiersService.CashiersServiceClient(channel);

        // Act
        var response = await client.GetCashierAsync(new GetCashierRequest { Id = expectedId.ToString() },
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.CashierId.ShouldBe(expectedId.ToString());
    }
}
