// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Extensions;

namespace Operations.Extensions.Abstractions.Messaging;

/// <summary>
///     Specifies the topic name for an integration event class.
/// </summary>
/// <remarks>
///     Apply this attribute to integration event classes to define their topic name
///     in the message broker. The topic name is used for routing messages to the
///     appropriate consumers. An optional domain can be specified to override the
///     assembly's default domain.
/// </remarks>
/// <example>
///     <code>
/// [EventTopic("user-created", domain: "users")]
/// public class UserCreatedEvent : IIntegrationEvent { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EventTopicAttribute(string topic, string? domain = null, string? version = "v1") : Attribute
{
    /// <summary>
    ///     Gets the topic name for the event.
    /// </summary>
    public string Topic { get; } = topic;

    /// <summary>
    ///     Gets the domain for the event.
    /// </summary>
    /// <value>
    ///     The domain name for this event, or <c>null</c> to use the assembly's default domain.
    /// </value>
    public string? Domain { get; } = domain;

    /// <summary>
    ///     Event major version (Default: 'v1')
    /// </summary>
    /// <remarks>When set, it will be appended at the end of the topic name</remarks>
    public string? Version { get; } = version;
}

/// <summary>
///     Specifies the topic name for an integration event class based on an entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type whose name will be used to generate the topic name.</typeparam>
/// <remarks>
///     This generic version automatically generates a topic name from the entity type name,
///     converting it to kebab-case. For example, <c>UserAccount</c> becomes <c>user-account</c>.
/// </remarks>
/// <example>
///     <code>
/// [EventTopic&lt;User&gt;(domain: "users")]
/// public class UserCreatedEvent : IIntegrationEvent { }
/// // Results in topic: "user"
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EventTopicAttribute<TEntity>(string? domain = null) : EventTopicAttribute(typeof(TEntity).Name.ToKebabCase(), domain);
