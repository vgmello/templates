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
    private const string ICommandOfTResultFullName = "Operations.Extensions.Messaging.ICommand`1"; // Note generic arity marker

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
            return null; // Should not happen if ForAttributeWithMetadataName works as expected

        string? sp = null;
        string? sql = null;
        bool useSnakeCase = false;
        bool returnsAffectedRecords = true; // Default value in attribute

        foreach (var arg in dbCommandAttribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case "Sp": sp = arg.Value.Value as string; break;
                case "Sql": sql = arg.Value.Value as string; break;
                case "UseSnakeCase": if (arg.Value.Value is bool bVal) useSnakeCase = bVal; break;
                case "ReturnsAffectedRecords": if (arg.Value.Value is bool rVal) returnsAffectedRecords = rVal; break;
            }
        }
        // Constructor arguments could also be used if attribute definition changes
        // For DbCommandAttribute(string? sp = null, string? sql = null, bool useSnakeCase = false, bool returnsAffectedRecords = true)
        // named arguments are typical if many are optional. If constructor args are used, logic needs to map positions.
        // Current DbCommandAttribute uses constructor with optional args.
        if (dbCommandAttribute.ConstructorArguments.Length > 0 && dbCommandAttribute.ConstructorArguments[0].Value is string spVal) sp = spVal;
        if (dbCommandAttribute.ConstructorArguments.Length > 1 && dbCommandAttribute.ConstructorArguments[1].Value is string sqlVal) sql = sqlVal;
        if (dbCommandAttribute.ConstructorArguments.Length > 2 && dbCommandAttribute.ConstructorArguments[2].Value is bool uscVal) useSnakeCase = uscVal;
        if (dbCommandAttribute.ConstructorArguments.Length > 3 && dbCommandAttribute.ConstructorArguments[3].Value is bool rarVal) returnsAffectedRecords = rarVal;


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
        string? genericArgumentResultTypeFullName = null; // For T in ICommand<T> or U in ICommand<IEnumerable<U>>
        bool isNonQuery = false;
        bool isEnumerableResult = false;

        var iCommandOfTResultInterface = typeSymbol.AllInterfaces.FirstOrDefault(i =>
            i.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{ICommandOfTResultFullName}");

        if (iCommandOfTResultInterface != null && iCommandOfTResultInterface.TypeArguments.FirstOrDefault() is INamedTypeSymbol typeArgument)
        {
            commandResultTypeFullName = typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            genericArgumentResultTypeFullName = commandResultTypeFullName; // Default to TResult

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
                isNonQuery = true;
            }
        }

        // If neither ICommand nor ICommand<TResult> is implemented, handler generation might be skipped or an error reported.
        // For now, ToDbParams is always generated. Handler relies on these flags.
        if (!isNonQuery && commandResultTypeFullName == null && (sp != null || sql != null))
        {
            // Report diagnostic: handler cannot be generated without ICommand or ICommand<TResult>
            // context.ReportDiagnostic(Diagnostic.Create(...));
            // For now, let GenerateHandlerPart decide based on these flags.
        }

        var containingTypes = GetContainingTypeDeclarations(typeSymbol);

        return new TypeInfo(
            Namespace: typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
            TypeName: typeSymbol.Name,
            TypeDeclaration: GetTypeDeclarationSyntax(typeSymbol),
            ContainingTypes: containingTypes,
            Sp: sp,
            Sql: sql,
            UseSnakeCase: useSnakeCase,
            ReturnsAffectedRecords: returnsAffectedRecords,
            Properties: properties,
            CommandResultTypeFullName: commandResultTypeFullName,
            GenericArgumentResultTypeFullName: genericArgumentResultTypeFullName,
            IsNonQuery: isNonQuery,
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
        sourceBuilder.AppendLine("}"); // Close class/struct

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
            return; // No handler if Sp or Sql is not provided

        if (!typeInfo.IsNonQuery && typeInfo.CommandResultTypeFullName == null)
        {
            // Cannot generate handler if it's not ICommand and not ICommand<TResult>
            // Optionally report a diagnostic here.
            return;
        }

        var handlerClassName = $"{typeInfo.TypeName}Handler";
        var sourceBuilder = new StringBuilder();

        sourceBuilder.AppendLine("// <auto-generated/>");
        sourceBuilder.AppendLine("#nullable enable");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("using System.Threading.Tasks;"); // For Task/Task<T>
        sourceBuilder.AppendLine("using Dapper;"); // For Dapper extension methods
        sourceBuilder.AppendLine("using System.Data;"); // For CommandType
        // Potentially add using for NpgsqlDataSource if not globally available
        // sourceBuilder.AppendLine("using Npgsql;");

        if (typeInfo.Namespace != null)
        {
            sourceBuilder.AppendLine($"namespace {typeInfo.Namespace};");
            sourceBuilder.AppendLine();
        }

        // Handler class is separate, not partial to the command type.
        // It could be nested within containing types if desired, but simpler as top-level in namespace.
        sourceBuilder.AppendLine($"public static class {handlerClassName}");
        sourceBuilder.AppendLine("{");

        string returnType;
        string dapperCall;
        string commandText = typeInfo.Sp ?? typeInfo.Sql!;
        string commandType = string.IsNullOrEmpty(typeInfo.Sp) ? "CommandType.Text" : "CommandType.StoredProcedure";

        if (typeInfo.IsNonQuery)
        {
            returnType = "Task";
            dapperCall = $"await connection.ExecuteAsync(new CommandDefinition(\"{commandText}\", dbParams, commandType: {commandType}, cancellationToken: cancellationToken));";
        }
        else
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

        sourceBuilder.AppendLine($"    public static async {returnType} HandleAsync({typeInfo.FullyQualifiedTypeName} command, global::Npgsql.NpgsqlDataSource dataSource, global::System.Threading.CancellationToken cancellationToken = default)");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine("        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);");
        sourceBuilder.AppendLine("        var dbParams = command.ToDbParams();");
        sourceBuilder.AppendLine($"        {dapperCall}");
        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine("}"); // Close class

        spc.AddSource($"{typeInfo.SafeFileName}.{handlerClassName}.g.cs", sourceBuilder.ToString());
    }

    // --- Helper Methods ---
    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && name[i - 1] != '_') sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }

    private static string GetTypeDeclarationSyntax(INamedTypeSymbol typeSymbol)
    {
        var kind = typeSymbol.TypeKind switch
        {
            TypeKind.Class => typeSymbol.IsRecord ? "partial record class" : "partial class",
            TypeKind.Struct => typeSymbol.IsRecord ? "partial record struct" : "partial struct",
            _ => "partial class" // Fallback, though attribute targets Class.
        };
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
        while (parent != null)
        {
            builder.Add(GetTypeDeclarationSyntax(parent)); // Use the same declaration style
            parent = parent.ContainingType;
        }
        builder.Reverse(); // Outermost to innermost
        return builder.ToImmutable();
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
        for (int i = 0; i < containingTypes.Length; i++)
        {
            sb.AppendLine("}");
        }
    }
}

// --- Data Structures ---
internal record TypeInfo(
    string? Namespace,
    string TypeName,
    string TypeDeclaration, // e.g. "public partial class MyCommand"
    ImmutableArray<string> ContainingTypes, // Declarations of containing types
    string? Sp,
    string? Sql,
    bool UseSnakeCase,
    bool ReturnsAffectedRecords,
    ImmutableArray<PropertyInfo> Properties,
    string? CommandResultTypeFullName, // TResult in ICommand<TResult>
    string? GenericArgumentResultTypeFullName, // U in ICommand<IEnumerable<U>> or T in ICommand<T>
    bool IsNonQuery, // True if implements non-generic ICommand
    bool IsEnumerableResult // True if TResult is IEnumerable<U>
)
{
    public string SafeFileName => $"{Namespace?.Replace('.', '_') ?? "global"}_{TypeName.Replace('<', '_').Replace('>', '_')}";
    public string FullyQualifiedTypeName => $"global::{(Namespace != null ? Namespace + "." : "")}{string.Join(".", ContainingTypes.Select(GetTypeNameFromDeclaration))}{ (ContainingTypes.Any() ? "." : "")}{TypeName}";

    private static string GetTypeNameFromDeclaration(string declaration)
    {
        // Basic parsing, assumes "accessibility modifiers keyword Name<Generics>"
        var parts = declaration.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        // Last part before potential generics is the name
        var namePart = parts.LastOrDefault(p => p != "{" && p != "}") ?? "";
        var genericTick = namePart.IndexOf('<');
        return genericTick > 0 ? namePart.Substring(0, genericTick) : namePart;
    }
}


internal record PropertyInfo(string PropertyName, string ParameterName, string PropertyTypeFullName);
