// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Cashier.Data.Entities;

[Table("cashiers")]
public record Cashier : Entity
{
    [Key]
    public Guid CashierId { get; set; }

    public int CashierNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }
}
