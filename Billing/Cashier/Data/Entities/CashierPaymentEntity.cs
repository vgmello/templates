using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Cashier.Data.Entities;

public class CashierPaymentEntity
{
    [Key]
    public int PaymentId { get; set; }
    public int CashierId { get; set; }
    [Required]
    public DateTime PaymentDate { get; set; }

    public DateTime CreatedDateUtc { get; set; }
    public DateTime UpdatedDateUtc { get; set; }
    [ConcurrencyCheck]
    public int Version { get; set; }

    [ForeignKey(nameof(CashierId))]
    public virtual CashierEntity Cashier { get; set; } = null!;
}