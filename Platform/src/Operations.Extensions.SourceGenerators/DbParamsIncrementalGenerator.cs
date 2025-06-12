// Copyright (c) ABCDEG. All rights reserved.
#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;

namespace Operations.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public class DbCommandSourceGenerator : IIncrementalGenerator
{
    private const string DbCommandAttributeFullName = "Operations.Extensions.Dapper.DbCommandAttribute";
    private const string ColumnAttributeFullName = "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute";
    private const string ICommandFullName = "Operations.Extensions.Messaging.ICommand";
    private const string ICommandOfTResultFullName = "Operations.Extensions.Messaging.ICommand`1";

    // Diagnostic Descriptor
    private static readonly DiagnosticDescriptor NonQueryWithGenericResultWarning = new DiagnosticDescriptor(
        id: "DBCOMMANDGEN001",
        title: "NonQuery attribute used with generic ICommand<TResult>",
        messageFormat: "DbCommandAttribute's NonQuery property is true for command '{0}' which implements ICommand<{1}>. This is unusual as NonQuery typically implies no data is returned. The generated handler will execute the command and attempt to return default({1}).",
        category: "DbCommandSourceGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DbCommandAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => ExtractTypeInfo(ctx, ct))
            .Where(static typeInfo => typeInfo is not null);

        context.RegisterSourceOutput(commandTypes, static (spc, typeInfo) =>
        {
            if (typeInfo is null) return;
            // Pass SourceProductionContext to ExtractTypeInfo for diagnostic reporting if needed,
            // or report diagnostics directly in GenerateHandlerPart.
            // For now, GenerateHandlerPart will handle its own diagnostics.
            GenerateDbOpsPart(spc, typeInfo);
            GenerateHandlerPart(spc, typeInfo);
        });
    }

    private static TypeInfo? ExtractTypeInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var dbCommandAttribute = typeSymbol.GetAttributes().FirstOrDefault(ad =>
            ad.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{DbCommandAttributeFullName}");

        if (dbCommandAttribute == null)
            return null;

        string? sp = null;
        string? sql = null;
        bool useSnakeCase = false; // Default from attribute constructor
        bool returnsAffectedRecords = true; // Default from attribute constructor
        bool nonQueryAttribute = false; // Default from attribute constructor

        // DbCommandAttribute constructor: DbCommandAttribute(string? sp = null, string? sql = null, bool useSnakeCase = false, bool returnsAffectedRecords = true, bool nonQuery = false)
        // Arguments are positional and resolved by Roslyn, including their default values if not specified by user.
        if (dbCommandAttribute.ConstructorArguments.Length > 0 && dbCommandAttribute.ConstructorArguments[0].Value is string spVal) sp = spVal;
        if (dbCommandAttribute.ConstructorArguments.Length > 1 && dbCommandAttribute.ConstructorArguments[1].Value is string sqlVal) sql = sqlVal;
        if (dbCommandAttribute.ConstructorArguments.Length > 2 && dbCommandAttribute.ConstructorArguments[2].Value is bool uscVal) useSnakeCase = uscVal;
        if (dbCommandAttribute.ConstructorArguments.Length > 3 && dbCommandAttribute.ConstructorArguments[3].Value is bool rarVal) returnsAffectedRecords = rarVal;
        if (dbCommandAttribute.ConstructorArguments.Length > 4 && dbCommandAttribute.ConstructorArguments[4].Value is bool nqVal) nonQueryAttribute = nqVal;

        // Also check NamedArguments, as user could specify them by name, which might override positional defaults
        // if the attribute was constructed that way (though less common for full-constructor attributes).
        // For an attribute with all-optional constructor args, named args are common.
        foreach (var arg in dbCommandAttribute.NamedArguments) // This path is less likely if all args are constructor args
        {
            switch (arg.Key)
            {
                case "Sp": sp = arg.Value.Value as string ?? sp; break;
                case "Sql": sql = arg.Value.Value as string ?? sql; break;
                case "UseSnakeCase": if (arg.Value.Value is bool bVal) useSnakeCase = bVal; break;
                case "ReturnsAffectedRecords": if (arg.Value.Value is bool rVal) returnsAffectedRecords = rVal; break;
                case "NonQuery": if (arg.Value.Value is bool nqVal) nonQueryAttribute = nqVal; break;
            }
        }

        var properties = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && p.GetMethod != null)
            .Select(p =>
            {
                var columnNameAttr = p.GetAttributes().FirstOrDefault(ad =>
                    ad.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{ColumnAttributeFullName}");
                string parameterName;
                if (columnNameAttr != null && columnNameAttr.ConstructorArguments.Any() && columnNameAttr.ConstructorArguments[0].Value is string customName)
                {
                    parameterName = customName;
                }
                else
                {
                    parameterName = useSnakeCase ? ToSnakeCase(p.Name) : p.Name;
                }
                return new PropertyInfo(p.Name, parameterName, p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            })
            .ToImmutableArray();

        string? commandResultTypeFullName = null;
        string? genericArgumentResultTypeFullName = null;
        bool isNonQueryType = false; // From ICommand interface
        bool isEnumerableResult = false;

        var iCommandOfTResultInterface = typeSymbol.AllInterfaces.FirstOrDefault(i =>
            i.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{ICommandOfTResultFullName}");

        if (iCommandOfTResultInterface != null && iCommandOfTResultInterface.TypeArguments.FirstOrDefault() is INamedTypeSymbol typeArgument)
        {
            commandResultTypeFullName = typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            genericArgumentResultTypeFullName = commandResultTypeFullName;

            if (typeArgument.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Collections.Generic.IEnumerable`1" &&
                typeArgument.TypeArguments.FirstOrDefault() is INamedTypeSymbol enumerableTypeArg)
            {
                isEnumerableResult = true;
                genericArgumentResultTypeFullName = enumerableTypeArg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }
        else
        {
            var iCommandInterface = typeSymbol.AllInterfaces.FirstOrDefault(i =>
                i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{ICommandFullName}");
            if (iCommandInterface != null)
            {
                isNonQueryType = true;
            }
        }

        var containingTypes = GetContainingTypeDeclarations(typeSymbol);

        return new TypeInfo(
            Symbol: typeSymbol, // Store symbol for location in diagnostic
            Namespace: typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
            TypeName: typeSymbol.Name,
            TypeDeclaration: GetTypeDeclarationSyntax(typeSymbol),
            ContainingTypes: containingTypes,
            Sp: sp,
            Sql: sql,
            UseSnakeCase: useSnakeCase,
            ReturnsAffectedRecords: returnsAffectedRecords,
            IsNonQueryAttribute: nonQueryAttribute, // New field
            Properties: properties,
            CommandResultTypeFullName: commandResultTypeFullName,
            GenericArgumentResultTypeFullName: genericArgumentResultTypeFullName,
            IsNonQueryType: isNonQueryType,
            IsEnumerableResult: isEnumerableResult
        );
    }

    private static void GenerateDbOpsPart(SourceProductionContext spc, TypeInfo typeInfo)
    {
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
        if (string.IsNullOrWhiteSpace(typeInfo.Sp) && string.IsNullOrWhiteSpace(typeInfo.Sql))
            return;

        if (!typeInfo.IsNonQueryType && typeInfo.CommandResultTypeFullName == null)
        {
            // Report diagnostic: Handler cannot be generated if not ICommand or ICommand<TResult>
            // For now, just skip generation. A diagnostic would be better.
            return;
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

        string returnType;
        string dapperCall;
        string commandText = typeInfo.Sp ?? typeInfo.Sql!;
        string commandType = string.IsNullOrEmpty(typeInfo.Sp) ? "CommandType.Text" : "CommandType.StoredProcedure";
        string defaultReturnForNonQueryGeneric = "";

        if (typeInfo.IsNonQueryAttribute)
        {
            // NonQuery attribute is true, overrides other logic for return type determination
            if (typeInfo.IsNonQueryType) // Implements ICommand
            {
                returnType = "Task";
                dapperCall = $"await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
            }
            else if (typeInfo.CommandResultTypeFullName == "global::System.Int32" || typeInfo.CommandResultTypeFullName == "int") // Implements ICommand<int>
            {
                returnType = "Task<int>";
                dapperCall = $"return await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));"; // Returns rows affected
            }
            else // Implements ICommand<TResult> where TResult is not int
            {
                // Issue diagnostic
                var location = typeInfo.Symbol.Locations.FirstOrDefault() ?? Location.None;
                spc.ReportDiagnostic(Diagnostic.Create(NonQueryWithGenericResultWarning, location, typeInfo.TypeName, typeInfo.CommandResultTypeFullName));

                returnType = $"Task<{typeInfo.CommandResultTypeFullName}>";
                dapperCall = $"await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                defaultReturnForNonQueryGeneric = $"return default({typeInfo.CommandResultTypeFullName});";
            }
        }
        else // NonQuery attribute is false, use existing logic
        {
            if (typeInfo.IsNonQueryType) // Implements ICommand
            {
                returnType = "Task";
                dapperCall = $"await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
            }
            else // Implements ICommand<TResult>
            {
                returnType = $"Task<{typeInfo.CommandResultTypeFullName}>";
                if (typeInfo.CommandResultTypeFullName == "global::System.Int32" || typeInfo.CommandResultTypeFullName == "int")
                {
                    if (typeInfo.ReturnsAffectedRecords)
                    {
                        dapperCall = $"return await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                    }
                    else
                    {
                        dapperCall = $"return await connection.ExecuteScalarAsync<int>(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                    }
                }
                else if (typeInfo.IsEnumerableResult)
                {
                    dapperCall = $"return await connection.QueryAsync<{typeInfo.GenericArgumentResultTypeFullName}>(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                }
                else // Single object result
                {
                    dapperCall = $"return await connection.QueryFirstOrDefaultAsync<{typeInfo.GenericArgumentResultTypeFullName}>(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
                }
            }
        }

        sourceBuilder.AppendLine($"    public static async {returnType} HandleAsync({typeInfo.FullyQualifiedTypeName} command, global::Npgsql.NpgsqlDataSource dataSource, global::System.Threading.CancellationToken cancellationToken = default)");
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

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++) { char c = name[i]; if (char.IsUpper(c)) { if (i > 0 && name[i - 1] != '_') sb.Append('_'); sb.Append(char.ToLowerInvariant(c)); } else sb.Append(c); }
        return sb.ToString();
    }

    private static string GetTypeDeclarationSyntax(INamedTypeSymbol typeSymbol)
    {
        var kind = typeSymbol.TypeKind switch { TypeKind.Class => typeSymbol.IsRecord ? "partial record class" : "partial class", TypeKind.Struct => typeSymbol.IsRecord ? "partial record struct" : "partial struct", _ => "partial class" };
        return $"{SyntaxFacts.GetText(typeSymbol.DeclaredAccessibility)} {kind} {typeSymbol.Name}{(typeSymbol.IsGenericType ? FormatGenerics(typeSymbol.TypeArguments) : "")}";
    }

    private static string FormatGenerics(ImmutableArray<ITypeSymbol> typeArguments)
    {
        if (typeArguments.IsEmpty) return string.Empty;
        return $"<{string.Join(", ", typeArguments.Select(ta => ta.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>";
    }

    private static ImmutableArray<string> GetContainingTypeDeclarations(INamedTypeSymbol typeSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var parent = typeSymbol.ContainingType;
        while (parent != null) { builder.Add(GetTypeDeclarationSyntax(parent)); parent = parent.ContainingType; }
        builder.Reverse(); return builder.ToImmutable();
    }

    private static void AppendContainingTypeStarts(StringBuilder sb, ImmutableArray<string> containingTypes)
    {
        foreach (var typeDecl in containingTypes) { sb.AppendLine(typeDecl); sb.AppendLine("{"); }
    }

    private static void AppendContainingTypeEnds(StringBuilder sb, ImmutableArray<string> containingTypes)
    {
        for (int i = 0; i < containingTypes.Length; i++) { sb.AppendLine("}"); }
    }
}

internal record TypeInfo(
    INamedTypeSymbol Symbol, // Added for diagnostic location
    string? Namespace,
    string TypeName,
    string TypeDeclaration,
    ImmutableArray<string> ContainingTypes,
    string? Sp,
    string? Sql,
    bool UseSnakeCase,
    bool ReturnsAffectedRecords,
    bool IsNonQueryAttribute, // New field
    ImmutableArray<PropertyInfo> Properties,
    string? CommandResultTypeFullName,
    string? GenericArgumentResultTypeFullName,
    bool IsNonQueryType, // Renamed from IsNonQuery for clarity
    bool IsEnumerableResult
)
{
    public string SafeFileName => $"{Namespace?.Replace('.', '_') ?? "global"}_{TypeName.Replace('<', '_').Replace('>', '_')}";
    // Updated FullyQualifiedTypeName to be more robust by directly using typeSymbol parts
    public string FullyQualifiedTypeName => ConstructFullyQualifiedName(Symbol);

    private static string ConstructFullyQualifiedName(INamedTypeSymbol typeSymbol)
    {
        var parts = new System.Collections.Generic.List<string>();
        var current = typeSymbol;
        while(current != null && !current.ContainingNamespace.IsGlobalNamespace)
        {
            parts.Add(current.Name + (current.IsGenericType ? FormatGenerics(current.TypeArguments) : ""));
            current = current.ContainingType;
        }
        parts.Reverse();
        string typeNameWithNesting = string.Join(".", parts);

        if (typeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            return $"global::{typeNameWithNesting}";
        }
        return $"global::{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeNameWithNesting}";
    }
     private static string FormatGenerics(ImmutableArray<ITypeSymbol> typeArguments) // Duplicated for local use
    {
        if (typeArguments.IsEmpty) return string.Empty;
        return $"<{string.Join(", ", typeArguments.Select(ta => ta.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>";
    }
}

internal record PropertyInfo(string PropertyName, string ParameterName, string PropertyTypeFullName);
