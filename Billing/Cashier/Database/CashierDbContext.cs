using Billing.Cashier.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Cashier.Database;

public class CashierDbContext(DbContextOptions<CashierDbContext> options) : DbContext(options)
{
    public DbSet<CashierEntity> Cashiers { get; set; } = null!;

    public DbSet<CashierPaymentEntity> CashierPayments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashierDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
