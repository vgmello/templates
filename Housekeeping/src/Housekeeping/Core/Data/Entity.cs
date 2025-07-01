// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.Core.Data;

public abstract class Entity
{
    public DateTimeOffset CreatedDateUtc { get; init; }
    public DateTimeOffset UpdatedDateUtc { get; init; }
    public int Version { get; init; }
}
