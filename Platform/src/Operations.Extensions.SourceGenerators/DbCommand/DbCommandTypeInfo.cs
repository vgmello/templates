// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Dapper;
using System.Collections.Immutable;

namespace Operations.Extensions.SourceGenerators.DbCommand;

/// <summary>
///     Abstract base class containing metadata about a type marked with DbCommandAttribute.
/// </summary>
/// <remarks>
///     This class is used by the source generator to hold all necessary information
///     about a command type for generating database handlers and parameter providers.
/// </remarks>
internal abstract class DbCommandTypeInfo(
    string name,
    string qualifiedTypeName,
    string typeDeclaration,
    DbCommandAttribute dbCommandAttribute)
{
    /// <summary>
    ///     Represents information about a property that will be mapped to a database parameter.
    /// </summary>
    internal class PropertyInfo(string propertyName, string parameterName)
    {
        /// <summary>
        ///     Gets the name of the property in the C# type.
        /// </summary>
        public string PropertyName { get; } = propertyName;

        /// <summary>
        ///     Gets the name of the database parameter (may differ due to case conversion or Column attribute).
        /// </summary>
        public string ParameterName { get; } = parameterName;
    }

    /// <summary>
    ///     Contains information about the result type of a command.
    /// </summary>
    internal class ResultTypeInfo(
        string typeName,
        string qualifiedTypeName,
        string genericArgumentResultFullTypeName,
        bool isIntegralType,
        bool isEnumerableResult)
    {
        /// <summary>
        ///     Gets the simple name of the result type.
        /// </summary>
        public string TypeName { get; } = typeName;

        /// <summary>
        ///     Gets the fully qualified name of the result type.
        /// </summary>
        public string QualifiedTypeName { get; } = qualifiedTypeName;

        /// <summary>
        ///     Gets the full type name of the generic argument if the result is a collection.
        /// </summary>
        public string GenericArgumentResultFullTypeName { get; } = genericArgumentResultFullTypeName;

        /// <summary>
        ///     Gets whether the result type is an integral type (int, long).
        /// </summary>
        public bool IsIntegralType { get; } = isIntegralType;

        /// <summary>
        ///     Gets whether the result is a collection type.
        /// </summary>
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
