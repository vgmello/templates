// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Dapper;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ColumnAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
