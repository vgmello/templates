// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;
using Operations.Extensions.SourceGenerators.Extensions;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;

namespace Operations.Extensions.SourceGenerators.DbCommand;

internal class SourceGenDbCommandTypeInfo : DbCommandTypeInfo
{
    internal static string DbCommandAttributeFullName { get; } = typeof(DbCommandAttribute).FullName!;

    private static string CommandInterfaceFullName { get; } = $"{typeof(ICommand<>).Namespace}.ICommand<TResult>";

    private static string ColumnAttributeFullName { get; } = typeof(ColumnAttribute).FullName!;

    public SourceGenDbCommandTypeInfo(INamedTypeSymbol typeSymbol, DbParamsCase defaultDbParamsCase) :
        base(
            typeSymbol.Name,
            typeSymbol.GetQualifiedName(),
            typeSymbol.GetTypeDeclaration(),
            GetDbCommandAttribute(typeSymbol))
    {
        Namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString();
        DbProperties = GetDbCommandObjProperties(typeSymbol, DbCommandAttribute.ParamsCase, defaultDbParamsCase);
        ResultType = GetDbCommandResultInfo(typeSymbol);
        ParentTypes = typeSymbol.GetContainingTypesTree();
        DiagnosticsToReport = ExecuteAnalyzers(typeSymbol);
    }

    public ImmutableArray<Diagnostic> DiagnosticsToReport { get; }

    private ImmutableArray<Diagnostic> ExecuteAnalyzers(INamedTypeSymbol typeSymbol)
    {
        var diagnostics = new List<Diagnostic>();

        DbCommandAnalyzers.ExecuteMissingInterfaceAnalyzer(typeSymbol, ResultType, DbCommandAttribute, diagnostics);
        DbCommandAnalyzers.ExecuteNonQueryWithNonIntegralResultAnalyzer(typeSymbol, ResultType, DbCommandAttribute, diagnostics);
        DbCommandAnalyzers.ExecuteSpAndSqlAnalyzer(typeSymbol, DbCommandAttribute, diagnostics);

        return diagnostics.ToImmutableArray();
    }

    private static DbCommandAttribute GetDbCommandAttribute(INamedTypeSymbol typeSymbol)
    {
        var attributeData = typeSymbol.GetAttribute(DbCommandAttributeFullName)!;

        var spValue = attributeData.GetConstructorArgument<string>(index: 0);
        var sqlValue = attributeData.GetConstructorArgument<string>(index: 1);
        var dbParamCaseValue = attributeData.GetConstructorArgument<int>(index: 2);
        var nonQueryValue = attributeData.GetConstructorArgument<bool>(index: 3);
        var dataSourceValue = attributeData.GetConstructorArgument<string>(index: 4);

        return new DbCommandAttribute(
            sp: spValue,
            sql: sqlValue,
            paramsCase: (DbParamsCase)dbParamCaseValue,
            nonQuery: nonQueryValue,
            dataSource: dataSourceValue);
    }

    private static ResultTypeInfo? GetDbCommandResultInfo(INamedTypeSymbol typeSymbol)
    {
        var iCommandInterface = typeSymbol.AllInterfaces.FirstOrDefault(i =>
            i.OriginalDefinition.ToDisplayString().StartsWith(CommandInterfaceFullName));

        if (iCommandInterface?.TypeArguments[0] is not INamedTypeSymbol commandResultType)
            return null;

        var commandResultFullTypeName = commandResultType.GetQualifiedName(withGlobalNamespace: true);
        var genericArgumentResultFullTypeName = commandResultFullTypeName;
        var isEnumerableResult = false;

        var implementsIEnumerable = commandResultType.OriginalDefinition.ImplementsIEnumerable();

        if (implementsIEnumerable && commandResultType.TypeArguments.FirstOrDefault() is INamedTypeSymbol enumerableTypeArg)
        {
            isEnumerableResult = true;
            genericArgumentResultFullTypeName = enumerableTypeArg.GetQualifiedName(withGlobalNamespace: true);
        }

        return new ResultTypeInfo(
            typeName: commandResultType.Name,
            qualifiedTypeName: commandResultFullTypeName,
            genericArgumentResultFullTypeName: genericArgumentResultFullTypeName,
            isIntegralType: commandResultType.IsIntegralType(),
            isEnumerableResult: isEnumerableResult);
    }

    private static ImmutableArray<PropertyInfo> GetDbCommandObjProperties(
        INamedTypeSymbol typeSymbol, DbParamsCase paramsCase, DbParamsCase defaultDbParamsCase)
    {
        return typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(prop => prop is { DeclaredAccessibility: Accessibility.Public, IsStatic: false, GetMethod: not null })
            .Select(prop => new PropertyInfo(prop.Name, GetParameterNameFromProperty(prop, paramsCase, defaultDbParamsCase)))
            .ToImmutableArray();

        static string GetParameterNameFromProperty(IPropertySymbol prop, DbParamsCase paramsCase, DbParamsCase defaultParamsCase)
        {
            var columnNameAttribute = prop.GetAttribute(ColumnAttributeFullName);
            var customColumnName = columnNameAttribute?.GetConstructorArgument<string>(index: 0);

            if (customColumnName is not null)
                return customColumnName;

            var effectiveParamsCase = paramsCase == DbParamsCase.Unset ? defaultParamsCase : paramsCase;

            return effectiveParamsCase switch
            {
                DbParamsCase.SnakeCase => prop.Name.ToSnakeCase(),
                _ => prop.Name
            };
        }
    }
}
