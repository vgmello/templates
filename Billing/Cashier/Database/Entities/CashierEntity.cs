using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Cashier.Database.Entities;

[Table("Cashiers")]
public record CashierEntity
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

    public ICollection<CashierPaymentEntity> CashierPayments { get; set; } = [];
}
