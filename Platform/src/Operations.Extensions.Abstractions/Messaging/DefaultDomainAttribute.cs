// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Messaging;

[AttributeUsage(AttributeTargets.Assembly)]
public class DefaultDomainAttribute : Attribute
{
    public string? Domain { get; set; }
}
