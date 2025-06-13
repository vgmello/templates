// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.CodeAnalysis;
using Operations.Extensions.SourceGenerators.Extensions;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Operations.Extensions.SourceGenerators.DbCommand;

internal record DbCommandAttributes(string? Sp, string? Sql, bool UseSnakeCase, bool NonQuery);

internal record DbPropertyInfo(string PropertyName, string ParameterName);

internal record DbCommandResultTypeInfo(
    string Name,
    string FullTypeName,
    string GenericArgumentResultFullTypeName,
    bool IsIntegralType,
    bool IsEnumerableResult);

internal record DbCommandTypeInfo(
    string? Namespace,
    string TypeName,
    string FullyQualifiedTypeName,
    string TypeDeclaration,
    ImmutableArray<string> ContainingTypes,
    ImmutableArray<DbPropertyInfo> Properties,
    DbCommandResultTypeInfo? CommandResultTypeInfo,
    DbCommandAttributes CommandAttributesValues,
    ImmutableList<Diagnostic> DiagnosticsToReport
)
{
    public string SafeFileName { get; } = $"{Namespace?.Replace('.', '_') ?? "global"}_{TypeName.Replace('<', '_').Replace('>', '_')}";

    public bool ImplementsICommandInterface => CommandResultTypeInfo is not null;
}

[Generator]
public class DbCommandSourceGenerator : IIncrementalGenerator
{
    private const string DbCommandAttributeFullName = "Operations.Extensions.Dapper.DbCommandAttribute";

    private const string ColumnAttributeFullName = "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute";

    private const string ICommandFullName = "Operations.Extensions.Messaging.ICommand`1";

    private const string IDbParamsProviderFullName = "Operations.Extensions.Dapper.IDbParamsProvider";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Debugger.Break();

        var commandTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: DbCommandAttributeFullName,
                predicate: static (_, _) => true, // all types with the attribute
                transform: static (ctx, _) => ExtractTypeInfo(ctx))
            .Where(static typeInfo => typeInfo is not null);

        context.RegisterSourceOutput(commandTypes, static (spc, typeInfo) =>
        {
            foreach (var diagnostic in typeInfo!.DiagnosticsToReport)
            {
                spc.ReportDiagnostic(diagnostic);
            }

            // Only proceed with generation if there are no errors
            if (typeInfo.DiagnosticsToReport.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return;
            }

            GenerateDbOpsPart(spc, typeInfo);
            GenerateHandlerPart(spc, typeInfo);
        });
    }

    private static DbCommandTypeInfo? ExtractTypeInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var dbCommandAttribute = typeSymbol.GetAttribute(DbCommandAttributeFullName);

        if (dbCommandAttribute is null)
            return null;

        var typeDeclaration = typeSymbol.GetTypeDeclaration();
        var containingTypes = typeSymbol.GetContainingTypeDeclarations();

        var dbCommandAttributeValues = GetDbCommandAttributeValues(dbCommandAttribute);
        var commandResultTypeInfo = GetDbCommandResultInfo(typeSymbol);
        var properties = GetDbCommandObjectProperties(typeSymbol, dbCommandAttributeValues);

        var diagnostics = new List<Diagnostic>();

        DbCommandAnalyzers.ExecuteMissingInterfaceAnalyzer(typeSymbol,
            commandResultTypeInfo, dbCommandAttributeValues, diagnostics);

        DbCommandAnalyzers.ExecuteNonQueryWithNonIntegralResultAnalyzer(typeSymbol,
            commandResultTypeInfo, dbCommandAttributeValues, diagnostics);

        return new DbCommandTypeInfo(
            Namespace: typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
            TypeName: typeSymbol.Name,
            FullyQualifiedTypeName: typeSymbol.GetFullyQualifiedName(),
            TypeDeclaration: typeDeclaration,
            ContainingTypes: containingTypes,
            Properties: properties,
            CommandResultTypeInfo: commandResultTypeInfo,
            CommandAttributesValues: dbCommandAttributeValues,
            DiagnosticsToReport: diagnostics.ToImmutableList()
        );
    }

    private static DbCommandAttributes GetDbCommandAttributeValues(AttributeData dbCommandAttribute)
    {
        var spValue = dbCommandAttribute.GetConstructorArgument<string>(index: 0);
        var sqlValue = dbCommandAttribute.GetConstructorArgument<string>(index: 1);
        var dbParamCase = dbCommandAttribute.GetConstructorArgument<int>(index: 2);
        var nonQueryValue = dbCommandAttribute.GetConstructorArgument<bool>(index: 3);

        foreach (var arg in dbCommandAttribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case "sp": spValue = arg.Value.Value as string ?? spValue; break;
                case "sql": sqlValue = arg.Value.Value as string ?? sqlValue; break;
                case "paramsCase":
                    if (arg.Value.Value is int dbParamCaseVal) dbParamCase = dbParamCaseVal;

                    break;
                case "nonQuery":
                    if (arg.Value.Value is bool nonQueryVal) nonQueryValue = nonQueryVal;

                    break;
            }
        }

        // TODO: Enable snake_case default from project settings
        var useSnakeCase = dbParamCase == 1;

        return new DbCommandAttributes(Sp: spValue, Sql: sqlValue, UseSnakeCase: useSnakeCase, NonQuery: nonQueryValue);
    }

    private static DbCommandResultTypeInfo? GetDbCommandResultInfo(INamedTypeSymbol typeSymbol)
    {
        var iCommandInterface = typeSymbol.AllInterfaces.FirstOrDefault(i =>
            i.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{ICommandFullName}");

        if (iCommandInterface?.TypeArguments[0] is not INamedTypeSymbol commandResultType)
            return null;

        var commandResultFullTypeName = commandResultType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var genericArgumentResultFullTypeName = commandResultFullTypeName;
        var isEnumerableResult = false;

        var implementsIEnumerable = commandResultType.OriginalDefinition.ImplementsIEnumerable();

        if (implementsIEnumerable && commandResultType.TypeArguments.FirstOrDefault() is INamedTypeSymbol enumerableTypeArg)
        {
            isEnumerableResult = true;
            genericArgumentResultFullTypeName = enumerableTypeArg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return new DbCommandResultTypeInfo(
            Name: commandResultType.Name,
            FullTypeName: commandResultFullTypeName,
            GenericArgumentResultFullTypeName: genericArgumentResultFullTypeName,
            IsIntegralType: commandResultType.IsIntegralType(),
            IsEnumerableResult: isEnumerableResult);
    }

    private static ImmutableArray<DbPropertyInfo> GetDbCommandObjectProperties(INamedTypeSymbol typeSymbol,
        DbCommandAttributes dbCommandAttributeValues)
    {
        return typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(prop => prop is { DeclaredAccessibility: Accessibility.Public, IsStatic: false, GetMethod: not null })
            .Select(prop => new DbPropertyInfo(
                PropertyName: prop.Name,
                ParameterName: GetParameterNameFromProperty(prop, dbCommandAttributeValues.UseSnakeCase))).ToImmutableArray();

        static string GetParameterNameFromProperty(IPropertySymbol prop, bool useSnakeCase)
        {
            var columnNameAttribute = prop.GetAttribute(ColumnAttributeFullName);
            var customColumnName = columnNameAttribute?.GetConstructorArgument<string>(index: 0);

            if (customColumnName is not null)
                return customColumnName;

            return useSnakeCase ? prop.Name.ToSnakeCase() : prop.Name;
        }
    }

    private static void GenerateDbOpsPart(SourceProductionContext spc, DbCommandTypeInfo dbCommandTypeInfo)
    {
        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine("// <auto-generated/>");
        sourceBuilder.AppendLine("#nullable enable");
        sourceBuilder.AppendLine();

        if (dbCommandTypeInfo.Namespace is not null)
        {
            sourceBuilder.AppendLine($"namespace {dbCommandTypeInfo.Namespace};");
            sourceBuilder.AppendLine();
        }

        AppendContainingTypeStarts(sourceBuilder, dbCommandTypeInfo.ContainingTypes);

        sourceBuilder.AppendLine($"{dbCommandTypeInfo.TypeDeclaration} : global::{IDbParamsProviderFullName}");
        sourceBuilder.AppendLine("{");

        GenerateToDbParamsMethod(sourceBuilder, dbCommandTypeInfo);

        sourceBuilder.AppendLine("}");

        AppendContainingTypeEnds(sourceBuilder, dbCommandTypeInfo.ContainingTypes);

        spc.AddSource($"{dbCommandTypeInfo.SafeFileName}.DbOps.g.cs", sourceBuilder.ToString());
    }

    private static void GenerateToDbParamsMethod(StringBuilder sb, DbCommandTypeInfo dbCommandTypeInfo)
    {
        sb.AppendLine("    public global::System.Object ToDbParams()");
        sb.AppendLine("    {");
        sb.AppendLine("        var p = new {");

        var properties = dbCommandTypeInfo.Properties.ToList();

        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var comma = i < properties.Count - 1 ? "," : string.Empty;
            sb.AppendLine($"            {prop.ParameterName} = this.{prop.PropertyName}{comma}");
        }

        sb.AppendLine("        };");
        sb.AppendLine("        return p;");
        sb.AppendLine("    }");
    }

    private static void GenerateHandlerPart(SourceProductionContext spc, DbCommandTypeInfo dbCommandTypeInfo)
    {
        if (string.IsNullOrWhiteSpace(dbCommandTypeInfo.CommandAttributesValues.Sp) &&
            string.IsNullOrWhiteSpace(dbCommandTypeInfo.CommandAttributesValues.Sql))
            return; // No handler if Sp or Sql is not provided

        // Not an ICommand
        if (dbCommandTypeInfo.CommandResultTypeInfo is null)
        {
            return;
        }

        var handlerClassName = $"{dbCommandTypeInfo.TypeName}Handler";
        var sourceBuilder = new StringBuilder();

        sourceBuilder.AppendLine("// <auto-generated/>");
        sourceBuilder.AppendLine("#nullable enable");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("using System.Threading.Tasks;");
        sourceBuilder.AppendLine("using Dapper;");
        sourceBuilder.AppendLine("using System.Data;");

        if (dbCommandTypeInfo.Namespace is not null)
        {
            sourceBuilder.AppendLine($"namespace {dbCommandTypeInfo.Namespace};");
            sourceBuilder.AppendLine();
        }

        var returnTypeDeclaration = $"Task<{dbCommandTypeInfo.CommandResultTypeInfo.FullTypeName}>";
        var dapperCall = CreateDapperCall(dbCommandTypeInfo.CommandResultTypeInfo, dbCommandTypeInfo.CommandAttributesValues);

        sourceBuilder.AppendLine($"public static class {handlerClassName}");
        sourceBuilder.AppendLine("{");
        sourceBuilder.AppendLine(
            $"    public static async {returnTypeDeclaration} HandleAsync({dbCommandTypeInfo.FullyQualifiedTypeName} command, global::Npgsql.NpgsqlDataSource dataSource, global::System.Threading.CancellationToken cancellationToken = default)");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine("        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);");
        sourceBuilder.AppendLine("        var dbParams = command.ToDbParams();");
        sourceBuilder.AppendLine($"       {dapperCall}");
        sourceBuilder.AppendLine("    }");

        sourceBuilder.AppendLine("}");

        spc.AddSource($"{dbCommandTypeInfo.SafeFileName}.{handlerClassName}.g.cs", sourceBuilder.ToString());
    }

    private static string CreateDapperCall(DbCommandResultTypeInfo resultTypeInfo, DbCommandAttributes commandAttributesValues)
    {
        var commandText = commandAttributesValues.Sp ?? commandAttributesValues.Sql!;
        var commandType = string.IsNullOrEmpty(commandAttributesValues.Sp)
            ? "CommandType.Text"
            : "CommandType.StoredProcedure";

        var commandDefinitionCall = $"new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, " +
                                    $"cancellationToken: cancellationToken)";

        // ICommand<short/int/long>
        if (resultTypeInfo.IsIntegralType)
        {
            if (commandAttributesValues.NonQuery)
            {
                return $"return await connection.ExecuteAsync({commandDefinitionCall});";
            }

            return $"return await connection" +
                   $".ExecuteScalarAsync<{resultTypeInfo.GenericArgumentResultFullTypeName}>({commandDefinitionCall});";
        }

        // ICommand<IEnumerable<TResul>>
        if (resultTypeInfo.IsEnumerableResult)
        {
            return $"return await connection.QueryAsync<{resultTypeInfo.GenericArgumentResultFullTypeName}>({commandDefinitionCall});";
        }

        // Single object result
        return $"return await connection" +
               $".QueryFirstOrDefaultAsync<{resultTypeInfo.GenericArgumentResultFullTypeName}>({commandDefinitionCall});";
    }

    private static void AppendContainingTypeStarts(StringBuilder sb, ImmutableArray<string> containingTypes)
    {
        foreach (var typeDecl in containingTypes)
        {
            sb.AppendLine(typeDecl);
            sb.AppendLine("{");
        }
    }

    private static void AppendContainingTypeEnds(StringBuilder sb, ImmutableArray<string> containingTypes)
    {
        for (var i = 0; i < containingTypes.Length; i++)
        {
            sb.AppendLine("}");
        }
    }
}
