// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;
using Dapper;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Operations.Extensions.Data.LinqToDb;

namespace Billing.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddBillingServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("BillingDb");

        builder.Services.AddLinqToDBContext<BillingDb>((svcProvider, options) =>
            options
                .UseMappingSchema(schema => schema.AddMetadataReader(new SnakeCaseNamingConventionMetadataReader()))
                .UsePostgreSQL(builder.Configuration.GetConnectionString("BillingDb")!)
                .UseDefaultLogging(svcProvider)
        );

        return builder;
    }
}
