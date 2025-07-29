// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.EventMarkdownGenerator.Models;
using Operations.Extensions.EventMarkdownGenerator.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Reflection;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Operations.Extensions.EventMarkdownGenerator;

public sealed class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-a|--assemblies")]
        [Description("Comma-separated list of assembly paths to analyze")]
        public required string Assemblies { get; init; }

        [CommandOption("--xml-docs")]
        [Description("Comma-separated list of XML documentation file paths (auto-discovered if not provided)")]
        public string? XmlDocs { get; init; }

        [CommandOption("-o|--output")]
        [Description("Output directory for generated markdown files")]
        public string? Output { get; init; }

        [CommandOption("--sidebar-file")]
        [Description("Name of the JSON sidebar file")]
        [DefaultValue("events-sidebar.json")]
        public string SidebarFile { get; init; } = "events-sidebar.json";

        [CommandOption("--templates")]
        [Description("Custom templates directory to override default templates")]
        public string? Templates { get; init; }

        [CommandOption("--github-url")]
        [Description("Base GitHub URL for source code links (e.g., https://github.com/org/repo/blob/main/src)")]
        public string? GitHubUrl { get; init; }

        [CommandOption("-v|--verbose")]
        [Description("Enable verbose output")]
        public bool Verbose { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var assemblyPaths = settings.Assemblies.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            var xmlDocPaths = string.IsNullOrEmpty(settings.XmlDocs)
                ? []
                : settings.XmlDocs.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList();

            var options = new GeneratorOptions
            {
                AssemblyPaths = assemblyPaths,
                XmlDocumentationPaths = xmlDocPaths,
                OutputDirectory = settings.Output ?? Environment.CurrentDirectory,
                SidebarFileName = settings.SidebarFile,
                TemplatesDirectory = settings.Templates,
                GitHubBaseUrl = settings.GitHubUrl
            };

            await GenerateDocumentationAsync(options);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");

            if (settings.Verbose)
            {
                AnsiConsole.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return 1;
        }
    }

    private static async Task GenerateDocumentationAsync(GeneratorOptions options)
    {
        if (options.AssemblyPaths.Count == 0)
            throw new ArgumentException("At least one assembly path must be provided", nameof(options));

        // Validate assembly paths
        var missingAssembly = options.AssemblyPaths.FirstOrDefault(path => !File.Exists(path));

        if (missingAssembly is not null)
        {
            throw new FileNotFoundException($"Assembly not found: {missingAssembly}");
        }

        options.EnsureOutputDirectoryExists();

        var xmlParser = new XmlDocumentationParser();
        var markdownGenerator = new FluidMarkdownGenerator(options.TemplatesDirectory);
        var sidebarGenerator = new JsonSidebarGenerator();

        var xmlDocumentationPaths = DiscoverXmlDocumentationFiles(options.AssemblyPaths, options.XmlDocumentationPaths);

        if (xmlDocumentationPaths.Count > 0)
        {
            await xmlParser.LoadMultipleDocumentationAsync(xmlDocumentationPaths);
        }

        var allEvents = new List<EventWithDocumentation>();
        var processedAssemblies = 0;

        foreach (var assemblyPath in options.AssemblyPaths)
        {
            try
            {
                var assembly = LoadAssemblyWithDependencyResolution(assemblyPath);
                var events = AssemblyEventDiscovery.DiscoverEvents(assembly, xmlParser);

                foreach (var eventMetadata in events)
                {
                    var documentation = xmlParser.GetEventDocumentation(eventMetadata.EventType);
                    var eventWithDoc = new EventWithDocumentation
                    {
                        Metadata = eventMetadata,
                        Documentation = documentation
                    };

                    allEvents.Add(eventWithDoc);
                }

                processedAssemblies++;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning:[/] Failed to process assembly {assemblyPath}: {ex.Message}");
            }
        }

        if (allEvents.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Warning:[/] No distributed events found in the provided assemblies.");
            AnsiConsole.WriteLine($"Processed {processedAssemblies} assemblies but found no types with EventTopic attributes.");

            // Still generate empty sidebar for consistency
            await sidebarGenerator.WriteSidebarAsync(new List<EventWithDocumentation>(), options.GetSidebarPath());
            AnsiConsole.MarkupLine($"[green]✓[/] Generated empty sidebar file: {options.SidebarFileName}");

            return;
        }

        // Generate individual markdown files
        var markdownFiles = markdownGenerator.GenerateAllMarkdown(allEvents, options.OutputDirectory, options).ToList();

        // Write markdown files
        foreach (var markdownFile in markdownFiles)
        {
            await File.WriteAllTextAsync(markdownFile.FilePath, markdownFile.Content);
        }

        // Extract and generate schema files
        var schemaTypes = new HashSet<Type>();

        foreach (var eventWithDoc in allEvents)
        {
            var eventSchemaTypes = TypeUtils.CollectComplexTypesFromProperties(eventWithDoc.Metadata.Properties);

            foreach (var schemaType in eventSchemaTypes)
            {
                schemaTypes.Add(schemaType);
                TypeUtils.CollectNestedComplexTypes(schemaType, schemaTypes);
            }
        }

        // Generate schema markdown files
        var schemaFiles = markdownGenerator.GenerateAllSchemas(schemaTypes, options.OutputDirectory).ToList();

        // Write schema files
        foreach (var schemaFile in schemaFiles)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(schemaFile.FilePath)!);
            await File.WriteAllTextAsync(schemaFile.FilePath, schemaFile.Content);
        }

        // Generate and write sidebar JSON
        await sidebarGenerator.WriteSidebarAsync(allEvents, options.GetSidebarPath());

        // Summary
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(
            $"[green]✓[/] Successfully generated documentation for [bold]{allEvents.Count}[/] events from [bold]{processedAssemblies}[/] assemblies");
        AnsiConsole.MarkupLine($"[green]✓[/] Created [bold]{markdownFiles.Count}[/] markdown files in: {options.OutputDirectory}");

        if (schemaFiles.Any())
        {
            AnsiConsole.MarkupLine($"[green]✓[/] Created [bold]{schemaFiles.Count}[/] schema files");
        }

        AnsiConsole.MarkupLine($"[green]✓[/] Generated sidebar file: {options.SidebarFileName}");
    }

    private static HashSet<string> DiscoverXmlDocumentationFiles(List<string> assemblyPaths, List<string> explicitXmlPaths)
    {
        var explicitPath = explicitXmlPaths.Where(File.Exists);
        var autoDiscoveredPath = assemblyPaths.Select(GetExpectedXmlDocumentationPath).Where(File.Exists);

        return explicitPath.Concat(autoDiscoveredPath).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
    }

    private static string GetExpectedXmlDocumentationPath(string assemblyPath)
    {
        // Replace .dll/.exe with .xml in the same directory
        var directory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyPath);

        return Path.Combine(directory, $"{fileNameWithoutExtension}.xml");
    }


    private static Assembly LoadAssemblyWithDependencyResolution(string assemblyPath)
    {
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);

        if (assemblyDirectory is not null)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                var assemblyName = new AssemblyName(args.Name);
                var potentialPath = Path.Combine(assemblyDirectory, assemblyName.Name + ".dll");

                if (File.Exists(potentialPath))
                {
                    return Assembly.LoadFrom(potentialPath);
                }

                return null;
            };
        }

        return Assembly.LoadFrom(assemblyPath);
    }
}
