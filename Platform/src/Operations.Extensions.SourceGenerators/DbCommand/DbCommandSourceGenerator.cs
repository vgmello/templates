// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.SourceGenerators.DbCommand.Writers;
using Operations.Extensions.SourceGenerators.Extensions;

namespace Operations.Extensions.SourceGenerators.DbCommand;

/// <summary>
///     Source generator that creates database command handlers and parameter providers for types marked with DbCommandAttribute.
/// </summary>
/// <remarks>
///     This generator:
///     <list type="bullet">
///         <item>Generates ToDbParams() extension methods for parameter mapping</item>
///         <item>Creates Wolverine command handlers for database operations</item>
///         <item>Supports stored procedures, SQL queries, and functions</item>
///         <item>Handles parameter case conversion (None, SnakeCase)</item>
///         <item>Respects Column attributes for custom parameter names</item>
///     </list>
///     The default parameter case can be configured via MSBuild property: DbCommandDefaultParamCase
/// </remarks>
[Generator]
public class DbCommandSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    ///     MSBuild property name for configuring the default parameter case convention.
    /// </summary>
    public const string DbCommandDefaultParamCase = nameof(DbCommandDefaultParamCase);

    /// <summary>
    ///     Initializes the incremental generator.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var defaultParamCaseProvider = context.AnalyzerConfigOptionsProvider
            .Select((o, _) => GetDefaultParamsCaseFromMsBuild(o));

        var commandTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: DbCommandTypeInfoSourceGen.DbCommandAttributeFullName,
                predicate: static (_, _) => true,
                transform: static (ctx, _) => ctx.TargetSymbol as INamedTypeSymbol)
            .Where(static typeInfo => typeInfo is not null)
            .Combine(defaultParamCaseProvider)
            .Select((combinedValues, _) =>
            {
                var (namedTypeSymbol, dbParamsCase) = combinedValues;

                return new DbCommandTypeInfoSourceGen(namedTypeSymbol!, dbParamsCase);
            });

        context.RegisterSourceOutput(commandTypes, static (spc, dbCommandTypeInfo) =>
        {
            dbCommandTypeInfo.DiagnosticsToReport.ForEach(spc.ReportDiagnostic);

            // Only proceed with generation if there are no errors
            if (dbCommandTypeInfo.HasErrors)
            {
                return;
            }

            GenerateDbExtensionsPart(spc, dbCommandTypeInfo);
            GenerateHandlerPart(spc, dbCommandTypeInfo);
        });
    }

    private static void GenerateDbExtensionsPart(SourceProductionContext spc, DbCommandTypeInfo dbCommandTypeInfo)
    {
        var generatedDbParamsSource = DbCommandDbParamsSourceGenWriter.Write(dbCommandTypeInfo);

        var fileName = dbCommandTypeInfo.QualifiedTypeName.GetFileName(dbCommandTypeInfo.IsGlobalType);
        spc.AddSource($"{fileName}.DbExt.g.cs", generatedDbParamsSource);
    }

    private static void GenerateHandlerPart(SourceProductionContext spc, DbCommandTypeInfo dbCommandTypeInfo)
    {
        if (string.IsNullOrWhiteSpace(dbCommandTypeInfo.DbCommandAttribute.Sp) &&
            string.IsNullOrWhiteSpace(dbCommandTypeInfo.DbCommandAttribute.Sql) &&
            string.IsNullOrWhiteSpace(dbCommandTypeInfo.DbCommandAttribute.Fn))
            return; // No handler needed if Sp, Sql, or Fn is not provided

        var generatedHandlerSource = DbCommandHandlerSourceGenWriter.Write(dbCommandTypeInfo);

        var fileName = GetHandlerFileName(dbCommandTypeInfo);
        spc.AddSource($"{fileName}.g.cs", generatedHandlerSource);
    }

    private static DbParamsCase GetDefaultParamsCaseFromMsBuild(AnalyzerConfigOptionsProvider optionsProvider)
    {
        if (optionsProvider.GlobalOptions.TryGetValue($"build_property.{DbCommandDefaultParamCase}", out var stringValue) &&
            Enum.TryParse<DbParamsCase>(stringValue, out var defaultParamsCase))
        {
            return defaultParamsCase;
        }

        return DbParamsCase.None;
    }

    private static string GetHandlerFileName(DbCommandTypeInfo dbCommandTypeInfo)
    {
        if (!dbCommandTypeInfo.IsNestedType)
        {
            return dbCommandTypeInfo.QualifiedTypeName.GetFileName(dbCommandTypeInfo.IsGlobalType);
        }

        var parentType = dbCommandTypeInfo.ParentTypes.Last();

        return parentType.GetQualifiedName().GetFileName(parentType.ContainingNamespace.IsGlobalNamespace);
    }
}
