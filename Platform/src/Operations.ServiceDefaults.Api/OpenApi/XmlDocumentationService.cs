// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Operations.ServiceDefaults.Api.OpenApi;

/// <summary>
///     Defines a service for loading and retrieving XML documentation comments.
/// </summary>
public interface IXmlDocumentationService
{
    /// <summary>
    ///     Asynchronously loads XML documentation from the specified file.
    /// </summary>
    /// <param name="xmlFilePath">The path to the XML documentation file.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     <c>true</c> if the documentation was loaded successfully; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> LoadDocumentationAsync(string xmlFilePath);

    /// <summary>
    ///     Gets the documentation for a specific member by its XML documentation name.
    /// </summary>
    /// <param name="memberName">The XML documentation member name (e.g., "T:Namespace.Type").</param>
    /// <returns>The documentation information if found; otherwise, <c>null</c>.</returns>
    XmlDocumentationInfo? GetDocumentation(string memberName);

    /// <summary>
    ///     Gets the documentation for a method.
    /// </summary>
    /// <param name="methodInfo">The method to get documentation for.</param>
    /// <returns>The documentation information if found; otherwise, <c>null</c>.</returns>
    XmlDocumentationInfo? GetMethodDocumentation(MethodInfo methodInfo);

    /// <summary>
    ///     Gets the documentation for a type.
    /// </summary>
    /// <param name="type">The type to get documentation for.</param>
    /// <returns>The documentation information if found; otherwise, <c>null</c>.</returns>
    XmlDocumentationInfo? GetTypeDocumentation(Type type);

    /// <summary>
    ///     Gets the documentation for a property.
    /// </summary>
    /// <param name="propertyInfo">The property to get documentation for.</param>
    /// <returns>The documentation information if found; otherwise, <c>null</c>.</returns>
    XmlDocumentationInfo? GetPropertyDocumentation(PropertyInfo propertyInfo);

    /// <summary>
    ///     Clears all cached documentation.
    /// </summary>
    void ClearCache();
}

/// <summary>
///     Provides services for loading and retrieving XML documentation comments from compiled assemblies.
/// </summary>
/// <remarks>
///     This service parses XML documentation files generated during compilation and provides
///     methods to retrieve documentation for types, methods, properties, and other members.
///     Documentation is cached for performance after initial loading.
/// </remarks>
public class XmlDocumentationService(ILogger<XmlDocumentationService> logger) : IXmlDocumentationService
{
    private readonly ConcurrentDictionary<string, XmlDocumentationInfo> _documentationCache = new();

    public async Task<bool> LoadDocumentationAsync(string xmlFilePath)
    {
        if (!File.Exists(xmlFilePath))
        {
            logger.LogWarning("XML documentation file not found: {FilePath}", xmlFilePath);

            return false;
        }

        try
        {
            await using var fileStream = new FileStream(xmlFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                useAsync: true);

            using var xmlReader = XmlReader.Create(fileStream, new XmlReaderSettings
            {
                Async = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ConformanceLevel = ConformanceLevel.Document
            });

            await ParseXmlDocumentationAsync(xmlReader);

            logger.LogInformation("Loaded XML documentation from {FilePath} with {Count} entries", xmlFilePath, _documentationCache.Count);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load XML documentation from {FilePath}", xmlFilePath);

            return false;
        }
    }

    public XmlDocumentationInfo? GetDocumentation(string memberName) => _documentationCache.GetValueOrDefault(memberName);

    public XmlDocumentationInfo? GetMethodDocumentation(MethodInfo methodInfo)
    {
        var memberName = GetMethodDocumentationName(methodInfo);

        return GetDocumentation(memberName);
    }

    public XmlDocumentationInfo? GetTypeDocumentation(Type type)
    {
        return GetDocumentation($"T:{type.FullName}");
    }

    public XmlDocumentationInfo? GetPropertyDocumentation(PropertyInfo propertyInfo)
    {
        return GetDocumentation($"P:{propertyInfo.DeclaringType?.FullName}.{propertyInfo.Name}");
    }

    public void ClearCache() => _documentationCache.Clear();

    private async Task ParseXmlDocumentationAsync(XmlReader reader)
    {
        while (await reader.ReadAsync())
        {
            if (reader is not { NodeType: XmlNodeType.Element, Name: "member" })
                continue;

            var nameAttribute = reader.GetAttribute("name");

            if (!string.IsNullOrEmpty(nameAttribute))
            {
                var docInfo = await ParseMemberDocumentationAsync(reader);

                if (docInfo is not null)
                {
                    _documentationCache.TryAdd(nameAttribute, docInfo);
                }
            }
        }
    }

    private static async Task<XmlDocumentationInfo?> ParseMemberDocumentationAsync(XmlReader reader)
    {
        var docInfo = new XmlDocumentationInfo();
        var hasContent = false;

        if (reader.IsEmptyElement)
            return null;

        while (await reader.ReadAsync())
        {
            if (reader is { NodeType: XmlNodeType.EndElement, Name: "member" })
                break;

            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name.ToLowerInvariant())
                {
                    case "summary":
                        docInfo.Summary = await ReadElementContentAsync(reader);
                        hasContent = true;

                        break;
                    case "remarks":
                        docInfo.Remarks = await ReadElementContentAsync(reader);
                        hasContent = true;

                        break;
                    case "returns":
                        docInfo.Returns = await ReadElementContentAsync(reader);
                        hasContent = true;

                        break;
                    case "param":
                        var paramName = reader.GetAttribute("name");

                        if (!string.IsNullOrEmpty(paramName))
                        {
                            var paramDoc = await ReadElementContentAsync(reader);
                            var paramExample = reader.GetAttribute("example");

                            docInfo.Parameters[paramName] = new XmlDocumentationInfo.ParameterInfo(paramDoc, paramExample);
                            hasContent = true;
                        }

                        break;
                    case "response":
                        var responseCode = reader.GetAttribute("code");
                        var responseDoc = await ReadElementContentAsync(reader);

                        if (!string.IsNullOrEmpty(responseCode))
                        {
                            docInfo.Responses[responseCode] = responseDoc;
                            hasContent = true;
                        }

                        break;
                    case "example":
                        docInfo.Example = await ReadElementContentAsync(reader);
                        hasContent = true;

                        break;
                    default:
                        // Unknown element
                        if (!reader.IsEmptyElement)
                        {
                            await reader.ReadAsync();
                        }

                        break;
                }
            }
        }

        return hasContent ? docInfo : null;
    }

    private static async Task<string?> ReadElementContentAsync(XmlReader reader)
    {
        if (reader.IsEmptyElement)
            return null;

        var content = new StringBuilder();

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.EndElement)
                break;

            if (reader.NodeType is XmlNodeType.Text or XmlNodeType.CDATA)
            {
                content.Append(reader.Value.Trim());
            }
        }

        return content.ToString();
    }

    private static string GetMethodDocumentationName(MethodInfo methodInfo)
    {
        var sb = new StringBuilder();

        sb.Append("M:");
        sb.Append(methodInfo.DeclaringType?.FullName);
        sb.Append('.');
        sb.Append(methodInfo.Name);

        if (methodInfo.GetParameters().Length > 0)
        {
            sb.Append('(');

            var parameters = methodInfo.GetParameters();

            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(GetTypeName(parameters[i].ParameterType));
            }

            sb.Append(')');
        }

        return sb.ToString();
    }

    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.FullName ?? type.Name;

        var genericTypeName = type.GetGenericTypeDefinition().FullName;
        var genericArgs = type.GetGenericArguments();

        var sb = new StringBuilder();

        sb.Append(genericTypeName?[..genericTypeName.IndexOf('`')]);
        sb.Append('{');

        for (var i = 0; i < genericArgs.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(GetTypeName(genericArgs[i]));
        }

        sb.Append('}');

        return sb.ToString();
    }
}
