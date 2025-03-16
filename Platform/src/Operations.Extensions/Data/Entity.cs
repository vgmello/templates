// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Operations.Extensions.Data;

public record Entity
{
    public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDateUtc { get; set; } = DateTime.UtcNow;

    [ConcurrencyCheck]
    public int Version { get; set; }
}
