// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Cashier.Data.Entities;

[Table("cashiers")]
public record Cashier
{
    [Key]
    public Guid CashierId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }

    public DateTime UpdatedDateUtc { get; set; }

    [ConcurrencyCheck]
    public int Version { get; set; }
}
