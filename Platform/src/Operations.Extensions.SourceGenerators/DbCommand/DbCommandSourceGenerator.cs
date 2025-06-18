// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.SourceGenerators.Extensions;
using System.Collections.Immutable;
using System.Text;

namespace Operations.Extensions.SourceGenerators.DbCommand;

[Generator]
public class DbCommandSourceGenerator : IIncrementalGenerator
{
    private static readonly string DbParamsProviderInterfaceFullName = "Operations.Extensions.Abstractions.Dapper.IDbParamsProvider";

    public const string DbCommandDefaultParamCase = nameof(DbCommandDefaultParamCase);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var defaultParamCaseProvider = context.AnalyzerConfigOptionsProvider
            .Select((o, _) => GetDefaultParamsCaseFromMsBuild(o));

        var commandTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: SourceGenDbCommandTypeInfo.DbCommandAttributeFullName,
                predicate: static (_, _) => true,
                transform: static (ctx, _) => ctx.TargetSymbol as INamedTypeSymbol)
            .Where(static typeInfo => typeInfo is not null)
            .Combine(defaultParamCaseProvider)
            .Select((combinedValues, _) =>
            {
                var (namedTypeSymbol, dbParamsCase) = combinedValues;

                return new SourceGenDbCommandTypeInfo(namedTypeSymbol!, dbParamsCase);
            });

        context.RegisterSourceOutput(commandTypes, static (spc, dbCommandTypeInfo) =>
        {
            foreach (var diagnostic in dbCommandTypeInfo!.DiagnosticsToReport)
            {
                spc.ReportDiagnostic(diagnostic);
            }

            // Only proceed with generation if there are no errors
            if (dbCommandTypeInfo.DiagnosticsToReport.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return;
            }

            GenerateDbExtensionsPart(spc, dbCommandTypeInfo);
            GenerateHandlerPart(spc, dbCommandTypeInfo);
        });
    }

    private static void GenerateDbExtensionsPart(SourceProductionContext spc, DbCommandTypeInfo dbCommandTypeInfo)
    {
        var sourceBuilder = new StringBuilder();

        AppendFileHeader(sourceBuilder);
        AppendNamespace(sourceBuilder, dbCommandTypeInfo.Namespace);
        AppendContainingTypeStarts(sourceBuilder, dbCommandTypeInfo.ParentTypes);

        sourceBuilder.AppendLine($"{dbCommandTypeInfo.TypeDeclaration} : global::{DbParamsProviderInterfaceFullName}");
        sourceBuilder.AppendLine("{");

        GenerateToDbParamsMethod(sourceBuilder, dbCommandTypeInfo);

        sourceBuilder.AppendLine("}");

        AppendContainingTypeEnds(sourceBuilder, dbCommandTypeInfo.ParentTypes);

        var baseFileName = GetFileName(dbCommandTypeInfo.QualifiedTypeName, dbCommandTypeInfo.IsGlobalType);

        spc.AddSource($"{baseFileName}.DbExt.g.cs", sourceBuilder.ToString());
    }

    private static void GenerateToDbParamsMethod(StringBuilder sb, DbCommandTypeInfo dbCommandTypeInfo)
    {
        sb.AppendLine("    public global::System.Object ToDbParams()");
        sb.AppendLine("    {");

        if (dbCommandTypeInfo.HasCustomDbPropertyNames)
        {
            sb.AppendLine("        var p = new");
            sb.AppendLine("        {");

            var properties = dbCommandTypeInfo.DbProperties.ToList();

            for (var i = 0; i < properties.Count - 1; i++)
            {
                var prop = properties[i];
                sb.AppendLine($"            {prop.ParameterName} = this.{prop.PropertyName},");
            }

            var lastProp = properties[^1];
            sb.AppendLine($"            {lastProp.ParameterName} = this.{lastProp.PropertyName}");

            sb.AppendLine("        };");
            sb.AppendLine("        return p;");
        }
        else
        {
            sb.AppendLine("        return this;");
        }

        sb.AppendLine("    }");
    }

    private static void GenerateHandlerPart(SourceProductionContext spc, DbCommandTypeInfo dbCommandTypeInfo)
    {
        if (string.IsNullOrWhiteSpace(dbCommandTypeInfo.DbCommandAttribute.Sp) &&
            string.IsNullOrWhiteSpace(dbCommandTypeInfo.DbCommandAttribute.Sql))
            return; // No handler needed if Sp or Sql is not provided

        var returnTypeDeclaration = dbCommandTypeInfo.ResultType is null
            ? "global::System.Threading.Tasks.Task"
            : $"global::System.Threading.Tasks.Task<{dbCommandTypeInfo.ResultType!.QualifiedTypeName}>";

        var dapperCall = CreateDapperCall(dbCommandTypeInfo.ResultType, dbCommandTypeInfo.DbCommandAttribute);

        var sourceBuilder = new StringBuilder();

        AppendFileHeader(sourceBuilder);
        AppendNamespace(sourceBuilder, dbCommandTypeInfo.Namespace);

        if (dbCommandTypeInfo.IsNestedType)
        {
            AppendContainingTypeStarts(sourceBuilder, dbCommandTypeInfo.ParentTypes);
        }
        else
        {
            var handlerClassName = $"{dbCommandTypeInfo.TypeName}Handler";
            sourceBuilder.AppendLine($"public static class {handlerClassName}");
            sourceBuilder.AppendLine("{");
        }

        var dataSourceKey = dbCommandTypeInfo.DbCommandAttribute.DataSource;
        var dataSourceParameterDeclaration = string.IsNullOrEmpty(dataSourceKey)
            ? "global::System.Data.Common.DbDataSource datasource"
            : $"[global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(\"{dataSourceKey}\")] global::System.Data.Common.DbDataSource datasource";

        sourceBuilder.AppendLine(
            $"    public static async {returnTypeDeclaration} HandleAsync(global::{dbCommandTypeInfo.QualifiedTypeName} command, {dataSourceParameterDeclaration}, global::System.Threading.CancellationToken cancellationToken = default)");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine("        await using var connection = await datasource.OpenConnectionAsync(cancellationToken);");
        sourceBuilder.AppendLine("        var dbParams = command.ToDbParams();");
        sourceBuilder.AppendLine($"        return await {dapperCall}");
        sourceBuilder.AppendLine("    }");

        if (dbCommandTypeInfo.IsNestedType)
        {
            AppendContainingTypeEnds(sourceBuilder, dbCommandTypeInfo.ParentTypes);
        }
        else
        {
            sourceBuilder.AppendLine("}");
        }

        var fileName = GetHandlerFileName(dbCommandTypeInfo);
        spc.AddSource($"{fileName}.g.cs", sourceBuilder.ToString());
    }

    private static string CreateDapperCall(DbCommandTypeInfo.ResultTypeInfo? resultTypeInfo, DbCommandAttribute commandAttributesValues)
    {
        var commandText = commandAttributesValues.Sp ?? commandAttributesValues.Sql!;
        var commandType = string.IsNullOrEmpty(commandAttributesValues.Sp)
            ? "System.Data.CommandType.Text"
            : "System.Data.CommandType.StoredProcedure";

        var commandDefinitionCall = $"new global::Dapper.CommandDefinition(\"{commandText}\", dbParams, commandType: global::{commandType}, " +
                                    "cancellationToken: cancellationToken)";

        var methodCall = CreateMethodDapperCall(resultTypeInfo, commandAttributesValues);

        return $"global::Dapper.SqlMapper.{methodCall}(connection, {commandDefinitionCall});";
    }

    private static string CreateMethodDapperCall(DbCommandTypeInfo.ResultTypeInfo? resultTypeInfo,
        DbCommandAttribute commandAttributesValues)
    {
        if (resultTypeInfo is null)
        {
            return "ExecuteAsync";
        }

        // ICommand<short/int/long>
        if (resultTypeInfo.IsIntegralType)
        {
            return commandAttributesValues.NonQuery
                ? "ExecuteAsync"
                : $"ExecuteScalarAsync<{resultTypeInfo.GenericArgumentResultFullTypeName}>";
        }

        // ICommand<IEnumerable<TResul>>
        if (resultTypeInfo.IsEnumerableResult)
        {
            return $"QueryAsync<{resultTypeInfo.GenericArgumentResultFullTypeName}>";
        }

        // Single object result
        return $"QueryFirstOrDefaultAsync<{resultTypeInfo.GenericArgumentResultFullTypeName}>";
    }

    private static void AppendContainingTypeStarts(StringBuilder sb, ImmutableArray<INamedTypeSymbol> containingTypes)
    {
        foreach (var type in containingTypes)
        {
            sb.AppendLine(type.GetTypeDeclaration());
            sb.AppendLine("{");
        }
    }

    private static void AppendContainingTypeEnds(StringBuilder sb, ImmutableArray<INamedTypeSymbol> containingTypes)
    {
        for (var i = 0; i < containingTypes.Length; i++)
        {
            sb.AppendLine("}");
        }
    }

    private static void AppendNamespace(StringBuilder sb, string? ns)
    {
        if (ns is not null)
        {
            sb.AppendLine($"namespace {ns};");
            sb.AppendLine();
        }
    }

    private static void AppendFileHeader(StringBuilder sb)
    {
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
    }

    private static DbParamsCase GetDefaultParamsCaseFromMsBuild(AnalyzerConfigOptionsProvider o)
    {
        if (o.GlobalOptions.TryGetValue($"build_property.{DbCommandDefaultParamCase}", out var stringValue) &&
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
            return GetFileName(dbCommandTypeInfo.QualifiedTypeName, dbCommandTypeInfo.IsGlobalType);
        }

        var parentType = dbCommandTypeInfo.ParentTypes.Last();

        return GetFileName(parentType.GetQualifiedName(), parentType.ContainingNamespace.IsGlobalNamespace);
    }

    private static string GetFileName(string fullTypeName, bool isGlobalNamespace)
    {
        var fileName = fullTypeName
            .Replace('.', '_')
            .Replace('<', '_')
            .Replace('>', '_');

        return isGlobalNamespace ? $"global_{fileName}" : fileName;
    }
}
