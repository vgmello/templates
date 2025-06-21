// Copyright (c) ABCDEG. All rights reserved.

using Billing.Tests.Integration._Internal;
using Billing.Tests.Integration._Internal.Containers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Serilog;
using Serilog.Events;
using System.Diagnostics.CodeAnalysis;
using Testcontainers.PostgreSql;
using Wolverine;
using Wolverine.Runtime;

namespace Billing.Tests.Integration;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly INetwork _network = new NetworkBuilder().Build();

    private readonly PostgreSqlContainer _postgres;

    public GrpcChannel GrpcChannel { get; private set; } = null!;

    public IntegrationTestFixture()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithNetwork(_network)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _network.CreateAsync();
        await _postgres.StartAsync();

        await using var liquibaseMigrationContainer = new LiquibaseMigrationContainer(_postgres.Name, _network);
        await liquibaseMigrationContainer.StartAsync();

        GrpcChannel = GrpcChannel.ForAddress(Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = Server.CreateHandler()
        });
    }

    public new async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DisposeAsync();
        await Log.CloseAndFlushAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(GetLoggerConfig(nameof(Billing)).CreateLogger());
        });

        builder.UseSetting("ConnectionStrings:BillingDb", _postgres.GetDbConnectionString("billing"));
        builder.UseSetting("ConnectionStrings:ServiceBus", _postgres.GetDbConnectionString("service_bus"));

        WolverineSetupExtensions.SkipServiceRegistration = true;

        builder.ConfigureServices((ctx, services) =>
        {
            RemoveHostedServices(services);

            services.AddWolverineWithDefaults(ctx.Configuration, opt => opt.ApplicationAssembly = typeof(Program).Assembly);
            services.AddTransient<IMessageBus>(sp => new MessageBus(sp.GetRequiredService<IWolverineRuntime>()));
        });

        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapGrpcServices(typeof(Program)));
        });
    }

    private static LoggerConfiguration GetLoggerConfig(string logNamespace) =>
        new LoggerConfiguration()
            .WriteTo.Sink(new XUnitSink())
            .MinimumLevel.Warning()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override(logNamespace, LogEventLevel.Debug);

    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();

        foreach (var hostedService in hostedServices)
            services.Remove(hostedService);
    }
}
