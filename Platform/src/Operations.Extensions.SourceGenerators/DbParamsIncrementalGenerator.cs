// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Text;

namespace Operations.Extensions.SourceGenerators;

internal record DbCommandAttributes(string? Sp, string? Sql, bool UseSnakeCase, bool NonQuery);

internal record PropertyInfo(string PropertyName, string ParameterName);

internal record DbCommandResultTypeInfo(string FullTypeName, string GenericArgumentResultFullTypeName, bool IsEnumerableResult);

[Generator]
public class DbCommandSourceGenerator : IIncrementalGenerator
{
    private const string DbCommandAttributeFullName = "Operations.Extensions.Dapper.DbCommandAttribute";

    private const string ColumnAttributeFullName = "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute";

    private const string ICommandFullName = "Operations.Extensions.Messaging.ICommand`1";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: DbCommandAttributeFullName,
                predicate: static (_, _) => true, // all types with the attribute
                transform: static (ctx, _) => ExtractTypeInfo(ctx))
            .Where(static typeInfo => typeInfo is not null);

        context.RegisterSourceOutput(commandTypes, static (spc, typeInfo) =>
        {
            if (typeInfo is null)
                return;

            foreach (var diagnostic in typeInfo.DiagnosticsToReport)
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

    private static TypeInfo? ExtractTypeInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var dbCommandAttribute = typeSymbol.GetAttribute(DbCommandAttributeFullName);

        if (dbCommandAttribute is null)
            return null;

        var containingTypes = GetContainingTypeDeclarations(typeSymbol);
        var commandResultTypeInfo = GetDbCommandResultInfo(typeSymbol);

        var dbCommandAttributeValues = GetDbCommandAttributeValues(dbCommandAttribute);
        var properties = GetDbCommandObjectProperties(typeSymbol, dbCommandAttributeValues);

        var diagnostics = new List<Diagnostic>();

        DbCommandSourceGeneratorAnalyzers.ExecuteMissingInterfaceAnalyzer(typeSymbol,
            commandResultTypeInfo, dbCommandAttributeValues, diagnostics);

        return new TypeInfo(
            Symbol: typeSymbol,
            Namespace: typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
            TypeName: typeSymbol.Name,
            TypeDeclaration: GetTypeDeclarationSyntax(typeSymbol),
            ContainingTypes: containingTypes,
            Properties: properties,
            CommandResultTypeInfo: commandResultTypeInfo,
            CommandAttributesValues: dbCommandAttributeValues,
            DiagnosticsToReport: diagnostics.ToImmutableList() // Add diagnostics list
        );
    }

    private static ImmutableArray<string> GetContainingTypeDeclarations(INamedTypeSymbol typeSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var parent = typeSymbol.ContainingType;

        while (parent is not null)
        {
            builder.Add(GetTypeDeclarationSyntax(parent));
            parent = parent.ContainingType;
        }

        builder.Reverse();

        return builder.ToImmutable();
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

        return new DbCommandResultTypeInfo(commandResultFullTypeName, genericArgumentResultFullTypeName, isEnumerableResult);
    }

    private static DbCommandAttributes GetDbCommandAttributeValues(AttributeData dbCommandAttribute)
    {
        var spValue = dbCommandAttribute.GetConstructorArgument<string>(index: 0);
        var sqlValue = dbCommandAttribute.GetConstructorArgument<string>(index: 1);
        var useSnakeCaseValue = dbCommandAttribute.GetConstructorArgument<bool?>(index: 2);
        var nonQueryValue = dbCommandAttribute.GetConstructorArgument<bool>(index: 3);

        foreach (var arg in dbCommandAttribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case "Sp": spValue = arg.Value.Value as string ?? spValue; break;
                case "Sql": sqlValue = arg.Value.Value as string ?? sqlValue; break;
                case "UseSnakeCase":
                    if (arg.Value.Value is bool useSnakeCaseVal) useSnakeCaseValue = useSnakeCaseVal;

                    break;
                case "NonQuery":
                    if (arg.Value.Value is bool nonQueryVal) nonQueryValue = nonQueryVal;

                    break;
            }
        }

        // TODO: Enable snake_case default from project settings
        var useSnakeCase = useSnakeCaseValue ?? false;

        return new DbCommandAttributes(Sp: spValue, Sql: sqlValue, UseSnakeCase: useSnakeCase, NonQuery: nonQueryValue);
    }

    private static ImmutableArray<PropertyInfo> GetDbCommandObjectProperties(INamedTypeSymbol typeSymbol,
        DbCommandAttributes dbCommandAttributeValues)
    {
        return typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(prop => prop is { DeclaredAccessibility: Accessibility.Public, IsStatic: false, GetMethod: not null })
            .Select(prop => new PropertyInfo(
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

    private static void GenerateDbOpsPart(SourceProductionContext spc, TypeInfo typeInfo)
    {
        // This part is always generated if the attribute is present.
        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine("// <auto-generated/>");
        sourceBuilder.AppendLine("#nullable enable");
        sourceBuilder.AppendLine();

        if (typeInfo.Namespace != null)
        {
            sourceBuilder.AppendLine($"namespace {typeInfo.Namespace};");
            sourceBuilder.AppendLine();
        }

        AppendContainingTypeStarts(sourceBuilder, typeInfo.ContainingTypes);

        sourceBuilder.AppendLine($"{typeInfo.TypeDeclaration} : global::Operations.Extensions.Dapper.IDbParamsProvider");
        sourceBuilder.AppendLine("{");
        GenerateToDbParamsMethod(sourceBuilder, typeInfo);
        sourceBuilder.AppendLine("}");

        AppendContainingTypeEnds(sourceBuilder, typeInfo.ContainingTypes);

        spc.AddSource($"{typeInfo.SafeFileName}.DbOps.g.cs", sourceBuilder.ToString());
    }

    private static void GenerateToDbParamsMethod(StringBuilder sb, TypeInfo typeInfo)
    {
        sb.AppendLine("    public global::Dapper.DynamicParameters ToDbParams()");
        sb.AppendLine("    {");
        sb.AppendLine("        var p = new global::Dapper.DynamicParameters();");

        foreach (var prop in typeInfo.Properties)
        {
            sb.AppendLine($"        p.Add(\"{prop.ParameterName}\", this.{prop.PropertyName});");
        }

        sb.AppendLine("        return p;");
        sb.AppendLine("    }");
    }

    private static void GenerateHandlerPart(SourceProductionContext spc, TypeInfo typeInfo)
    {
        if (string.IsNullOrWhiteSpace(typeInfo.CommandAttributesValues.Sp) &&
            string.IsNullOrWhiteSpace(typeInfo.CommandAttributesValues.Sql))
            return; // No handler if Sp or Sql is not provided

        if (typeInfo.CommandResultTypeInfo is null)
        {
            return; // No ICommand<TResult> implemented on class having DbCommandAttribute
        }

        var handlerClassName = $"{typeInfo.TypeName}Handler";
        var sourceBuilder = new StringBuilder();

        sourceBuilder.AppendLine("// <auto-generated/>");
        sourceBuilder.AppendLine("#nullable enable");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("using System.Threading.Tasks;");
        sourceBuilder.AppendLine("using Dapper;");
        sourceBuilder.AppendLine("using System.Data;");

        if (typeInfo.Namespace != null)
        {
            sourceBuilder.AppendLine($"namespace {typeInfo.Namespace};");
            sourceBuilder.AppendLine();
        }

        sourceBuilder.AppendLine($"public static class {handlerClassName}");
        sourceBuilder.AppendLine("{");

        var commandText = typeInfo.CommandAttributesValues.Sp ?? typeInfo.CommandAttributesValues.Sql!;
        var commandType = string.IsNullOrEmpty(typeInfo.CommandAttributesValues.Sql)
            ? "CommandType.Text"
            : "CommandType.StoredProcedure";
        var defaultReturnForNonQueryGeneric = "";

        string returnType;
        string dapperCall;

        var resultTypeInfo = typeInfo.CommandResultTypeInfo;

        if (resultTypeInfo.FullTypeName is "global::System.Int32" or "int") // ICommand<int>
        {
            returnType = "Task<int>";

            if (typeInfo.CommandAttributesValues.NonQuery)
            {
                dapperCall =
                    $"return await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}," +
                    $" cancellationToken: cancellationToken));";
            }
            else
            {
                dapperCall =
                    $"return await connection.ExecuteScalarAsync<int>(new CommandDefinition(\"{commandText}\", dbParams," +
                    $" commandType: {commandType}, cancellationToken: cancellationToken));";
            }
        }
        else // ICommand<TResult> where TResult is not int (or if TResult is a "unit" type representing void)
        {
            returnType = $"Task<{typeInfo.CommandResultTypeFullName}>";

            if (typeInfo.IsNonQueryAttribute) // NonQuery = true for non-int TResult
            {
                // Diagnostic is reported by Initialize's output action based on TypeInfo.DiagnosticsToReport
                // No need to call spc.ReportDiagnostic here again if it's already in typeInfo.
                // However, the plan was to report DBCOMMANDGEN001 from here.
                // Let's ensure it's reported via spc from where it has context.
                var location = typeInfo.Symbol.Locations.FirstOrDefault() ?? Location.None;
                spc.ReportDiagnostic(Diagnostic.Create(NonQueryWithGenericResultWarning, location, typeInfo.TypeName,
                    typeInfo.CommandResultTypeFullName));

                dapperCall =
                    $"await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                defaultReturnForNonQueryGeneric = $"return default({typeInfo.CommandResultTypeFullName});";
            }
            else // NonQuery = false (standard query for ICommand<TResult>)
            {
                if (typeInfo.IsEnumerableResult)
                {
                    dapperCall =
                        $"return await connection.QueryAsync<{typeInfo.GenericArgumentResultTypeFullName}>(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                }
                else // Single object result
                {
                    dapperCall =
                        $"return await connection.QueryFirstOrDefaultAsync<{typeInfo.GenericArgumentResultTypeFullName}>(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                }
            }
        }

        sourceBuilder.AppendLine(
            $"    public static async {returnType} HandleAsync({typeInfo.FullyQualifiedTypeName} command, global::Npgsql.NpgsqlDataSource dataSource, global::System.Threading.CancellationToken cancellationToken = default)");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine("        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);");
        sourceBuilder.AppendLine("        var dbParams = command.ToDbParams();");
        sourceBuilder.AppendLine($"        {dapperCall}");

        if (!string.IsNullOrEmpty(defaultReturnForNonQueryGeneric))
        {
            sourceBuilder.AppendLine($"        {defaultReturnForNonQueryGeneric}");
        }

        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine("}");

        spc.AddSource($"{typeInfo.SafeFileName}.{handlerClassName}.g.cs", sourceBuilder.ToString());
    }

    private static string GetTypeDeclarationSyntax(INamedTypeSymbol typeSymbol)
    {
        var kind = typeSymbol.TypeKind switch
        {
            TypeKind.Class => typeSymbol.IsRecord ? "partial record class" : "partial class",
            TypeKind.Struct => typeSymbol.IsRecord ? "partial record struct" : "partial struct", _ => "partial class"
        };

        return
            $"{SyntaxFacts.GetText(typeSymbol.DeclaredAccessibility)} {kind} {typeSymbol.Name}{(typeSymbol.IsGenericType ? FormatGenerics(typeSymbol.TypeArguments) : "")}";
    }

    private static string FormatGenerics(ImmutableArray<ITypeSymbol> typeArguments)
    {
        if (typeArguments.IsEmpty) return string.Empty;

        return $"<{string.Join(", ", typeArguments.Select(ta => ta.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>";
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
        for (var i = 0; i < containingTypes.Length; i++) { sb.AppendLine("}"); }
    }
}

// Updated TypeInfo: Removed IsNonQueryType, added DiagnosticsToReport
internal record TypeInfo(
    INamedTypeSymbol Symbol,
    string? Namespace,
    string TypeName,
    string TypeDeclaration,
    ImmutableArray<string> ContainingTypes,
    ImmutableArray<PropertyInfo> Properties,
    DbCommandResultTypeInfo? CommandResultTypeInfo,
    DbCommandAttributes CommandAttributesValues,
    ImmutableList<Diagnostic> DiagnosticsToReport
)
{
    public string SafeFileName => $"{Namespace?.Replace('.', '_') ?? "global"}_{TypeName.Replace('<', '_').Replace('>', '_')}";
    public string FullyQualifiedTypeName => ConstructFullyQualifiedName(Symbol);

    private static string ConstructFullyQualifiedName(INamedTypeSymbol typeSymbol)
    {
        var parts = new List<string>();
        var currentType = typeSymbol;

        while (currentType != null)
        {
            var partName = currentType.Name + (currentType.IsGenericType ? FormatGenerics(currentType.TypeArguments) : "");
            parts.Add(partName);
            currentType = currentType.ContainingType;
        }

        parts.Reverse();
        var typeNameWithNesting = string.Join(".", parts);

        if (typeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            return $"global::{typeNameWithNesting}";
        }

        return $"global::{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeNameWithNesting}";
    }

    private static string FormatGenerics(ImmutableArray<ITypeSymbol> typeArguments)
    {
        if (typeArguments.IsEmpty) return string.Empty;

        return $"<{string.Join(", ", typeArguments.Select(ta => ta.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>";
    }
}

public static class SourceGeneratorExtensions
{
    public static T? GetConstructorArgument<T>(this AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments.Length > index && attribute.ConstructorArguments[index].Value is T argValue)
        {
            return argValue;
        }

        return default;
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, string attributeFullName)
    {
        return symbol.GetAttributes().FirstOrDefault(ad =>
            ad.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{attributeFullName}");
    }

    public static bool ImplementsIEnumerable(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(i =>
            i.Name == "IEnumerable" &&
            (i.ContainingNamespace.ToDisplayString() == "System.Collections" ||
             i.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"));
    }

    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0 && value[i - 1] != '_') sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else sb.Append(c);
        }

        return sb.ToString();
    }
}
