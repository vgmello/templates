// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.EventMarkdownGenerator.Models;
using Operations.Extensions.EventMarkdownGenerator.Services;
using Shouldly;
using System.Reflection;
using Xunit;

namespace Operations.Extensions.EventMarkdownGenerator.Tests;

public class IntegrationTests
{
    private static string TestAssemblyPath => FindTestAssemblyPath();
    private static string TestXmlPath => Path.ChangeExtension(TestAssemblyPath, ".xml");
    private static string ReferenceMarkdownPath => FindReferenceMarkdownPath();

    private static string FindTestAssemblyPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "tests", "TestEvents", "bin", "Debug", "net9.0", "TestEvents.dll"),
            Path.Combine(Path.GetDirectoryName(typeof(IntegrationTests).Assembly.Location)!,
                "..", "..", "..", "..", "TestEvents", "bin", "Debug", "net9.0", "TestEvents.dll")
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);

            if (File.Exists(fullPath))
                return fullPath;
        }

        throw new FileNotFoundException("Could not find TestEvents.dll. Make sure TestEvents project is built.");
    }

    private static string FindReferenceMarkdownPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs", "events", "cashier-created.md"),
            Path.Combine(Path.GetDirectoryName(typeof(IntegrationTests).Assembly.Location)!,
                "..", "..", "..", "..", "..", "..", "..", "docs", "events", "cashier-created.md")
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);

            if (File.Exists(fullPath))
                return fullPath;
        }

        // Return a dummy path if reference doesn't exist
        return Path.Combine(Path.GetTempPath(), "dummy-cashier-created.md");
    }

    [Fact]
    public async Task GenerateMarkdown_ShouldMatchReferenceFormat()
    {
        // Arrange
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDir);

        try
        {
            // Initialize services
            var xmlParser = new XmlDocumentationParser();
            var markdownGenerator = new FluidMarkdownGenerator();

            // Load XML documentation
            await xmlParser.LoadMultipleDocumentationAsync([TestXmlPath]);

            // Load and discover events
            var assembly = Assembly.LoadFrom(TestAssemblyPath);
            var events = AssemblyEventDiscovery.DiscoverEvents(assembly, xmlParser).ToList();

            events.Count.ShouldBeGreaterThan(0);
            var cashierCreatedEvent = events.FirstOrDefault(e => e.EventName == "CashierCreated");
            cashierCreatedEvent.ShouldNotBeNull();

            // Get documentation and generate markdown
            var documentation = xmlParser.GetEventDocumentation(cashierCreatedEvent.EventType);
            var eventWithDoc = new EventWithDocumentation
            {
                Metadata = cashierCreatedEvent,
                Documentation = documentation
            };

            var generatedMarkdown = markdownGenerator.GenerateMarkdown(eventWithDoc, outputDir);

            // Act - Read generated content
            var generatedContent = await File.ReadAllTextAsync(generatedMarkdown.FilePath, TestContext.Current.CancellationToken);

            // Debug output
            Console.WriteLine("Generated content:");
            Console.WriteLine(generatedContent);
            await File.WriteAllTextAsync("/tmp/debug-generated-content.txt", generatedContent, TestContext.Current.CancellationToken);

            // Assert - Compare with reference (if exists)
            if (File.Exists(ReferenceMarkdownPath))
            {
                var referenceContent = await File.ReadAllTextAsync(ReferenceMarkdownPath, TestContext.Current.CancellationToken);

                // Compare key sections
                CompareMarkdownSection(generatedContent, referenceContent, "# CashierCreated", "Title should match");
                CompareMarkdownSection(generatedContent, referenceContent, "**Status:**", "Status should match");
                CompareMarkdownSection(generatedContent, referenceContent, "**Topic:**", "Topic should match");
                CompareMarkdownSection(generatedContent, referenceContent, "**Type:**", "Type should match");
            }

            // Validate specific content regardless of reference
            ValidateGeneratedContent(generatedContent);
        }
        finally
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public void EventDiscovery_ShouldDetectPartitionKeys()
    {
        // Arrange
        var assembly = Assembly.LoadFrom(TestAssemblyPath);

        // Act
        var xmlParser = new XmlDocumentationParser();
        var events = AssemblyEventDiscovery.DiscoverEvents(assembly, xmlParser).ToList();

        // Assert
        events.Count.ShouldBe(1);
        var cashierEvent = events[0];

        cashierEvent.PartitionKeys.Count.ShouldBe(2);
        cashierEvent.PartitionKeys.ShouldContain(pk => pk.Name == "TenantId");
        cashierEvent.PartitionKeys.ShouldContain(pk => pk.Name == "PartitionKeyTest");

        // Validate partition key ordering
        var orderedKeys = cashierEvent.PartitionKeys.OrderBy(pk => pk.Order).ToList();
        orderedKeys[0].Name.ShouldBe("TenantId");
        orderedKeys[1].Name.ShouldBe("PartitionKeyTest");
    }

    [Fact]
    public void EventDiscovery_ShouldDetectDomainFromNamespace()
    {
        // Arrange
        var assembly = Assembly.LoadFrom(TestAssemblyPath);

        // Act
        var xmlParser = new XmlDocumentationParser();
        var events = AssemblyEventDiscovery.DiscoverEvents(assembly, xmlParser).ToList();

        // Assert
        events.Count.ShouldBe(1);
        var cashierEvent = events[0];
        cashierEvent.Domain.ShouldBe("Cashiers"); // Now returns subdomain instead of domain
    }

    [Fact]
    public async Task XmlDocumentationParser_ShouldParsePropertyDescriptions()
    {
        // Arrange
        var parser = new XmlDocumentationParser();
        await parser.LoadMultipleDocumentationAsync([TestXmlPath]);

        var assembly = Assembly.LoadFrom(TestAssemblyPath);
        var cashierCreatedType = assembly.GetType("Billing.Cashiers.Contracts.IntegrationEvents.CashierCreated");

        // Act
        var documentation = parser.GetEventDocumentation(cashierCreatedType!);

        // Assert
        documentation.Summary.ShouldContain("Published when a new cashier is successfully created");

        // Verify property descriptions
        documentation.PropertyDescriptions.ShouldContainKey("TenantId");
        documentation.PropertyDescriptions["TenantId"].ShouldBe("Identifier of the tenant that owns the cashier");

        documentation.PropertyDescriptions.ShouldContainKey("PartitionKeyTest");
        documentation.PropertyDescriptions["PartitionKeyTest"].ShouldBe("Additional partition key for message routing");

        documentation.PropertyDescriptions.ShouldContainKey("Cashier");
        documentation.PropertyDescriptions["Cashier"].ShouldBe("Complete cashier object containing all cashier data and configuration");
    }

    private static void CompareMarkdownSection(string generated, string reference, string sectionMarker, string message)
    {
        var generatedLine = FindLineContaining(generated, sectionMarker);
        var referenceLine = FindLineContaining(reference, sectionMarker);

        if (generatedLine != null && referenceLine != null)
        {
            generatedLine.ShouldBe(referenceLine, message);
        }
    }

    private static string? FindLineContaining(string content, string marker)
    {
        return content.Split('\n')
            .FirstOrDefault(line => line.Contains(marker))?.Trim();
    }

    [Fact]
    public void JsonSidebarGenerator_ShouldGenerateCorrectStructure()
    {
        // Arrange
        var assembly = Assembly.LoadFrom(TestAssemblyPath);
        var sidebarGenerator = new JsonSidebarGenerator();

        var xmlParser = new XmlDocumentationParser();
        var events = AssemblyEventDiscovery.DiscoverEvents(assembly, xmlParser).ToList();
        var eventsWithDoc = events.Select(e => new EventWithDocumentation
        {
            Metadata = e,
            Documentation = new EventDocumentation { Summary = "Test" }
        }).ToList();

        // Act
        var sidebarItems = sidebarGenerator.GenerateSidebarItems(eventsWithDoc);

        // Assert
        sidebarItems.Count.ShouldBe(2); // Events + Schemas sections

        // Validate events section
        var eventsSection = sidebarItems.FirstOrDefault(s => s.Text == "Cashiers");
        eventsSection.ShouldNotBeNull();
        eventsSection!.Items.Count.ShouldBe(1);
        eventsSection.Items[0].Text.ShouldBe("Cashier Created");
        eventsSection.Items[0].Link.ShouldBe("/cashier-created");

        // Validate schemas section
        var schemasSection = sidebarItems.FirstOrDefault(s => s.Text == "Schemas");
        schemasSection.ShouldNotBeNull();
        schemasSection!.Items.Count.ShouldBe(1);
        schemasSection.Items[0].Text.ShouldBe("Cashier");
    }

    private static void ValidateGeneratedContent(string content)
    {
        // Validate basic structure
        content.ShouldContain("# CashierCreated");
        content.ShouldContain("**Status:**");
        content.ShouldContain("**Version:**");
        content.ShouldContain("**Topic:**");
        content.ShouldContain("**Type:** Integration Event");

        // Validate sections
        content.ShouldContain("## Description");
        content.ShouldContain("## Event Payload");
        content.ShouldContain("### Partition Keys");
        content.ShouldContain("### Reference Schemas");
        content.ShouldContain("## Technical Details");

        // Validate partition keys are mentioned
        content.ShouldContain("TenantId");
        content.ShouldContain("PartitionKeyTest");

        // Validate frontmatter
        content.ShouldContain("---\neditLink: false\n---");

        // Validate topic format
        content.ShouldContain("**Topic:** `{env}.billing.public.cashiers.v1`");

        // Validate entity field
        content.ShouldContain("**Entity:** `cashier`");

        // Validate domain detection
        content.ShouldContain("Billing.Cashiers.Contracts.IntegrationEvents");
    }
}
