// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Tests.Architecture;

#pragma warning disable CS8602

public class DomainIsolationRulesTests : ArchitectureTestBase
{
    [Fact]
    public void Domains_ShouldNotDirectlyReferencEachOthersInternals()
    {
        var domainPrefixes = new[] { "Billing.Cashiers", "Billing.Invoices" };

        foreach (var domain in domainPrefixes)
        {
            var otherDomains = domainPrefixes.Where(d => d != domain).ToArray();

            var result = GetBillingTypes()
                .That().ResideInNamespace(domain)
                .And().DoNotResideInNamespaceEndingWith(".Contracts") // Contracts can be shared
                .Should().NotHaveDependencyOnAny(otherDomains.SelectMany(d => new[]
                {
                    $"{d}.Commands",
                    $"{d}.Queries",
                    $"{d}.Data"
                }).ToArray())
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(
                $"Domain {domain} should not directly reference other domains' internals: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }
}
