// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Invoices.Data.Entities;

[Table("invoices")]
public record Invoice : Entity
{
    [Key]
    public Guid InvoiceId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
