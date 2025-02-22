using System.ComponentModel.DataAnnotations;

namespace Billing.Cashier.Data.Entities;

public record CashierEntity
{
    [Key]
    public int CashierId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }
    public DateTime UpdatedDateUtc { get; set; }
    [ConcurrencyCheck]
    public int Version { get; set; }

    public virtual ICollection<CashierPaymentEntity> CashierPayments { get; set; } = new List<CashierPaymentEntity>();
}
