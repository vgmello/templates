using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Invoices.Data.Entities;

[Table("Invoices")]
public record Invoice
{
    [Key]
    public Guid InvoiceId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }

    public DateTime UpdatedDateUtc { get; set; }

    [ConcurrencyCheck]
    public int Version { get; set; }
}
