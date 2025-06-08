// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using Billing.Cashier.Queries;
using DomainCashier = Billing.Contracts.Cashier.Models.Cashier;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using Grpc.Net.Client;
using Wolverine;
using Shouldly;

namespace Billing.Tests.Integration;

public class CashierServiceTests
{
    [Fact]
    public async Task GetCashier_ReturnsCashier()
    {
        // Arrange
        var bus = Substitute.For<IMessageBus>();
        var expectedId = Guid.NewGuid();

        bus.InvokeAsync<DomainCashier>(Arg.Any<GetCashierQuery>(), Arg.Any<CancellationToken>())
            .Returns(new DomainCashier { CashierId = expectedId });

        bus.InvokeAsync<IEnumerable<GetCashiersQuery.Result>>(Arg.Any<GetCashiersQuery>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, cfg) =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ServiceBus:ConnectionString"] = string.Empty
                    });
                });
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(bus);
                    var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
                    foreach (var hostedService in hostedServices)
                        services.Remove(hostedService);
                });
            });

        var client = CreateClient(factory);

        // Act
        var response = await client.GetCashierAsync(new GetCashierRequest { Id = expectedId.ToString() },
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.CashierId.ShouldBe(expectedId.ToString());
    }

    private static Cashiers.CashiersClient CreateClient(WebApplicationFactory<Program> factory)
    {
        var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpClient = factory.CreateDefaultClient()
        });

        return new Cashiers.CashiersClient(channel);
    }
}
