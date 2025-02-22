using Billing.Cashier.Data.Configurations;
using Billing.Cashier.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Cashier.Data;

public class CashierDbContext : DbContext
{
    public CashierDbContext(DbContextOptions<CashierDbContext> options)
        : base(options)
    {
    }

    public DbSet<CashierEntity> Cashiers { get; set; } = null!;
    public DbSet<CashierPaymentEntity> CashierPayments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");

        modelBuilder.ApplyConfiguration(new CashierEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CashierPaymentEntityConfiguration());
    }
}
