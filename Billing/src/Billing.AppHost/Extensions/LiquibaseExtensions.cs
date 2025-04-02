// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.AppHost.Extensions;

public static class LiquibaseExtensions
{
    public static IResourceBuilder<ContainerResource> AddLiquibaseMigrations(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> dbServerResource,
        IResourceBuilder<ParameterResource> dbPassword)
    {
        return builder
            .AddContainer("liquibase", "liquibase/liquibase")
            .WithBindMount("../../infra/Billing.Database/Liquibase", "/liquibase/changelog")
            .WithEnvironment("LIQUIBASE_COMMAND_USERNAME", "postgres")
            .WithEnvironment("LIQUIBASE_COMMAND_PASSWORD", dbPassword)
            .WithEnvironment("LIQUIBASE_COMMAND_CHANGELOG_FILE", "changelog.xml")
            .WithEnvironment("LIQUIBASE_SEARCH_PATH", "/liquibase/changelog")
            .WithReference(dbServerResource)
            .WithEntrypoint("/bin/sh")
            .WithArgs("-c",
                """
                liquibase --url=jdbc:postgresql://billing-db:5432/postgres --contexts=@setup update && \
                liquibase --url=jdbc:postgresql://billing-db:5432/service_bus --contexts=@service_bus update && \
                liquibase --url=jdbc:postgresql://billing-db:5432/billing update
                """);
    }
}
