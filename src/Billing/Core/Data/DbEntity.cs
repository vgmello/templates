// Copyright (c) ABCDEG. All rights reserved.

using LinqToDB.Concurrency;
using LinqToDB.Mapping;

namespace Billing.Core.Data;

[Table(Schema = "billing")]
public abstract record DbEntity
{
    [Column(SkipOnUpdate = true)]
    public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDateUtc { get; set; } = DateTime.UtcNow;

    [Column("xmin", SkipOnInsert = true, SkipOnUpdate = true)]
    [OptimisticLockProperty(VersionBehavior.Auto)]
    public int Version { get; init; } = 0;
}
