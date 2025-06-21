// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Dapper;
using System.Collections.Immutable;

namespace Operations.Extensions.SourceGenerators.DbCommand;

internal abstract class DbCommandTypeInfo(
    string name,
    string qualifiedTypeName,
    string typeDeclaration,
    DbCommandAttribute dbCommandAttribute)
{
    internal class PropertyInfo(string propertyName, string parameterName)
    {
        public string PropertyName { get; } = propertyName;

        public string ParameterName { get; } = parameterName;
    }

    internal class ResultTypeInfo(
        string typeName,
        string qualifiedTypeName,
        string genericArgumentResultFullTypeName,
        bool isIntegralType,
        bool isEnumerableResult)
    {
        public string TypeName { get; } = typeName;
        public string QualifiedTypeName { get; } = qualifiedTypeName;
        public string GenericArgumentResultFullTypeName { get; } = genericArgumentResultFullTypeName;
        public bool IsIntegralType { get; } = isIntegralType;
        public bool IsEnumerableResult { get; } = isEnumerableResult;
    }

    public string? Namespace { get; protected set; }

    public string TypeName { get; } = name;

    public string QualifiedTypeName { get; } = qualifiedTypeName;

    public string TypeDeclaration { get; } = typeDeclaration;

    public DbCommandAttribute DbCommandAttribute { get; } = dbCommandAttribute;

    public ImmutableArray<PropertyInfo> DbProperties { get; protected set; }

    public ResultTypeInfo? ResultType { get; protected set; }

    public bool ImplementsICommandInterface => ResultType is not null;

    public ImmutableArray<INamedTypeSymbol> ParentTypes { get; protected set; }

    public bool IsNestedType => ParentTypes.Length > 0;

    public bool IsGlobalType => string.IsNullOrEmpty(Namespace);

    public bool HasCustomDbPropertyNames => DbProperties.Any(p => p.ParameterName != p.PropertyName);
}
