using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Operations.SourceGenerators
{

[Generator]
public class DbParamsIncrementalGenerator : IIncrementalGenerator
{
    private const string DbParamsAttributeName = "Operations.Extensions.Dapper.DbParamsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Use ForAttributeWithMetadataName for efficient attribute-based filtering
        var typesToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: DbParamsAttributeName,
                predicate: static (node, _) => true, // We want all types with the attribute
                transform: static (context, cancellationToken) => ExtractTypeInfo(context, cancellationToken))
            .Where(static x => x is not null);

        // Register source output with proper cancellation token support
        context.RegisterSourceOutput(typesToGenerate, static (context, typeInfo) => 
        {
            if (typeInfo.HasValue)
                GenerateSource(context, typeInfo.Value);
        });
    }

    private static TypeInfo? ExtractTypeInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        // Extract all equatable information from the symbol instead of storing the symbol itself
        var properties = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(static p => p.DeclaredAccessibility == Accessibility.Public)
            .Select(static p => p.Name)
            .OrderBy(static x => x) // Ensure deterministic ordering
            .ToImmutableArray();

        // Build containing type hierarchy as strings
        var containingTypes = ImmutableArray<string>.Empty;
        var parent = typeSymbol.ContainingType;
        var typesBuilder = ImmutableArray.CreateBuilder<string>();
        
        while (parent != null)
        {
            typesBuilder.Add(GetTypeDeclaration(parent));
            parent = parent.ContainingType;
        }
        
        if (typesBuilder.Count > 0)
        {
            typesBuilder.Reverse();
            containingTypes = typesBuilder.ToImmutable();
        }

        return new TypeInfo(
            Namespace: typeSymbol.ContainingNamespace.IsGlobalNamespace 
                ? null 
                : typeSymbol.ContainingNamespace.ToDisplayString(),
            TypeName: typeSymbol.Name,
            TypeDeclaration: GetTargetTypeDeclaration(typeSymbol),
            ContainingTypes: containingTypes,
            Properties: properties
        );
    }

    private static void GenerateSource(SourceProductionContext context, TypeInfo typeInfo)
    {
        var sourceCode = BuildSourceCode(typeInfo);
        var fileName = GetSafeFileName(typeInfo);
        
        context.AddSource($"{fileName}.DbParams.g.cs", sourceCode);
    }

    private static string BuildSourceCode(TypeInfo typeInfo)
    {
        var sb = new StringBuilder();

        // Add nullable enable directive for consistency
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Add namespace if present
        if (typeInfo.Namespace is not null)
        {
            sb.AppendLine($"namespace {typeInfo.Namespace};");
            sb.AppendLine();
        }

        // Add containing types with their original accessibility
        foreach (var containingType in typeInfo.ContainingTypes)
        {
            sb.AppendLine(containingType);
            sb.AppendLine("{");
        }

        // Add the main type declaration
        sb.AppendLine($"partial {typeInfo.TypeDeclaration} : Operations.Extensions.Dapper.IDbParamsProvider");
        sb.AppendLine("{");
        
        // Add ToDbParams method
        sb.AppendLine("    public Dapper.DynamicParameters ToDbParams()");
        sb.AppendLine("    {");
        sb.AppendLine("        var p = new Dapper.DynamicParameters();");

        foreach (var propertyName in typeInfo.Properties)
        {
            var parameterName = ToSnakeCase(propertyName);
            sb.AppendLine($"        p.Add(\"{parameterName}\", {propertyName});");
        }

        sb.AppendLine("        return p;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // Close containing types
        for (int i = 0; i < typeInfo.ContainingTypes.Length; i++)
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private static string GetTypeDeclaration(INamedTypeSymbol symbol)
    {
        string keyword;
        if (symbol.IsRecord)
        {
            keyword = symbol.IsValueType ? "record struct" : "record";
        }
        else
        {
            keyword = symbol.TypeKind switch
            {
                TypeKind.Struct => "struct",
                TypeKind.Interface => "interface",
                _ => "class"
            };
        }

        // For containing types, we need "static partial" ordering, not "partial static"
        var modifiers = symbol.IsStatic ? "static partial" : "partial";
        var accessibility = GetAccessibility(symbol);
        return $"{accessibility} {modifiers} {keyword} {symbol.Name}";
    }

    private static string GetTargetTypeDeclaration(INamedTypeSymbol symbol)
    {
        string keyword;
        if (symbol.IsRecord)
        {
            keyword = symbol.IsValueType ? "record struct" : "record";
        }
        else
        {
            keyword = symbol.TypeKind switch
            {
                TypeKind.Struct => "struct",
                TypeKind.Interface => "interface",
                _ => "class"
            };
        }

        // For target types, we just need the keyword and name, partial is added in template
        return $"{keyword} {symbol.Name}";
    }

    private static string GetSafeFileName(TypeInfo typeInfo)
    {
        var fullName = typeInfo.Namespace is not null 
            ? $"{typeInfo.Namespace}.{typeInfo.TypeName}"
            : typeInfo.TypeName;

        return fullName
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_")
            .Replace(" ", "")
            .Replace(".", "_");
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder();

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (char.IsUpper(c))
            {
                if (i > 0) 
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static string GetAccessibility(INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            _ => "private"
        };
    }

    // Value-equatable record that contains only primitive/equatable data
    private readonly record struct TypeInfo(
        string? Namespace,
        string TypeName,
        string TypeDeclaration,
        ImmutableArray<string> ContainingTypes,
        ImmutableArray<string> Properties
    );
}
}