// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Dapper;

namespace Billing.BackOffice.Orleans;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        SnakeCaseMappingExtensions.UseSnakeCaseMapping();

        builder.AddNpgsqlDataSource("BillingDb");

        return builder;
    }
}
