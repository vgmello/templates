// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Operations.Extensions.SourceGenerators.Extensions;

public static class SourceGeneratorExtensions
{
    private static SymbolDisplayFormat FullyQualifiedFormatNoGlobal { get; } =
        new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

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
        return symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == attributeFullName);
    }

    public static string GetTypeDeclaration(this INamedTypeSymbol typeSymbol)
    {
        var kind = typeSymbol.TypeKind switch
        {
            TypeKind.Class => typeSymbol.IsRecord ? "partial record" : "partial class",
            TypeKind.Struct => typeSymbol.IsRecord ? "partial record struct" : "partial struct",
            _ => "partial class"
        };

        var genericArgDefinition = typeSymbol.IsGenericType ? typeSymbol.TypeArguments.GetGenericsDeclaration() : string.Empty;

        return $"{SyntaxFacts.GetText(typeSymbol.DeclaredAccessibility)} {kind} {typeSymbol.Name}{genericArgDefinition}";
    }

    public static string GetQualifiedName(this ITypeSymbol typeSymbol, bool withGlobalNamespace = false) =>
        typeSymbol.ToDisplayString(withGlobalNamespace ? SymbolDisplayFormat.FullyQualifiedFormat : FullyQualifiedFormatNoGlobal);

    public static ImmutableArray<INamedTypeSymbol> GetContainingTypesTree(this ITypeSymbol typeSymbol)
    {
        var arrayBuilder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        var parent = typeSymbol.ContainingType;

        while (parent is not null)
        {
            arrayBuilder.Add(parent);
            parent = parent.ContainingType;
        }

        arrayBuilder.Reverse();

        return arrayBuilder.ToImmutable();
    }

    public static string GetGenericsDeclaration(this ImmutableArray<ITypeSymbol> typeArguments)
    {
        if (typeArguments.IsEmpty)
            return string.Empty;

        return $"<{string.Join(", ", typeArguments.Select(ta => ta.GetQualifiedName(withGlobalNamespace: true)))}>";
    }

    /// <summary>
    ///     Check if type is integer or long (integral in C# terminology).
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static bool IsIntegralType(this ITypeSymbol symbol) =>
        symbol.SpecialType switch
        {
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            _ => false
        };

    public static bool ImplementsIEnumerable(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(i =>
            i.Name == "IEnumerable" &&
            (
                i.ContainingNamespace.ToDisplayString() == "System.Collections" ||
                i.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
            )
        );
    }
}
