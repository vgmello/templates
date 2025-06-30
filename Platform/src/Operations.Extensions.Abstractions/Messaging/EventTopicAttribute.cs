// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Extensions;

namespace Operations.Extensions.Abstractions.Messaging;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EventTopicAttribute(string topic, string? domain = null) : Attribute
{
    public string Topic { get; } = topic;

    public string? Domain { get; } = domain;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EventTopicAttribute<TEntity>(string? domain = null) : EventTopicAttribute(typeof(TEntity).Name.ToKebabCase(), domain);
