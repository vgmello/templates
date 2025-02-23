// Copyright (c) ABCDEG Limited. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Billing.Infrastructure.Database;

public class BillingDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "../Billing.AppHost");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(appSettingsPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets("4cf68e53-c914-4dd3-aa99-9d1c9e31c02a")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("BillingDatabase");

        var optionsBuilder = new DbContextOptionsBuilder<BillingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new BillingDbContext(optionsBuilder.Options);
    }
}
