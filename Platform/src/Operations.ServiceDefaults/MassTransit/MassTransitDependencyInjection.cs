// Copyright (c) ABCDEG. All rights reserved.

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Operations.ServiceDefaults.MassTransit;

public static class MassTransitDependencyInjection
{
    public static IHostApplicationBuilder AddMassTransit(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("MassTransit");
            options.ConnectionString = connectionString;
        });

        if (builder.Environment.IsDevelopment())
            builder.Services.AddPostgresMigrationHostedService();

        builder.Services.AddMassTransit(m =>
        {
            m.AddSqlMessageScheduler();
            m.UsingPostgres((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}
