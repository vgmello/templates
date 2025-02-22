using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Cashier.Database.Entities;

[Table("CashierPayments")]
public class CashierPaymentEntity
{
    [Key]
    public Guid PaymentId { get; set; }

    public Guid CashierId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public DateTime UpdatedDateUtc { get; set; }

    [ConcurrencyCheck]
    public int Version { get; set; }

    [ForeignKey(nameof(CashierId))]
    public CashierEntity Cashier { get; set; } = null!;
}
