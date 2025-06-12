// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Operations.Extensions.SourceGenerators.Extensions;

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

    public static string GetTypeDeclaration(this INamedTypeSymbol typeSymbol)
    {
        var kind = typeSymbol.TypeKind switch
        {
            TypeKind.Class => typeSymbol.IsRecord ? "partial record class" : "partial class",
            TypeKind.Struct => typeSymbol.IsRecord ? "partial record struct" : "partial struct",
            _ => "partial class"
        };

        var genericArgDefinition = typeSymbol.IsGenericType ? typeSymbol.TypeArguments.GetGenericsDeclaration() : string.Empty;

        return $"{SyntaxFacts.GetText(typeSymbol.DeclaredAccessibility)} {kind} {typeSymbol.Name}{genericArgDefinition}";
    }

    public static string GetFullyQualifiedName(this INamedTypeSymbol typeSymbol)
    {
        var parts = new List<string>();
        var currentType = typeSymbol;

        while (currentType is not null)
        {
            var partName = currentType.Name + (currentType.IsGenericType ? currentType.TypeArguments.GetGenericsDeclaration() : "");
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

    public static ImmutableArray<string> GetContainingTypeDeclarations(this INamedTypeSymbol typeSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var parent = typeSymbol.ContainingType;

        while (parent is not null)
        {
            builder.Add(parent.GetTypeDeclaration());
            parent = parent.ContainingType;
        }

        builder.Reverse();

        return builder.ToImmutable();
    }

    public static string GetGenericsDeclaration(this ImmutableArray<ITypeSymbol> typeArguments)
    {
        if (typeArguments.IsEmpty)
            return string.Empty;

        return $"<{string.Join(", ", typeArguments.Select(ta => ta.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>";
    }
}
