// Copyright (c) ABCDEG. All rights reserved.

using Billing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<BillingDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("BillingDatabase")));

        return builder;
    }
}
