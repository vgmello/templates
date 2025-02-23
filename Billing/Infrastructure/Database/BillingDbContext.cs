using Billing.Cashier.Data.Entities;
using Billing.Invoices.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Database;

public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Cashier.Data.Entities.Cashier> Cashiers { get; set; } = null!;

    public DbSet<CashierCurrency> CashierPayments { get; set; } = null!;

    public DbSet<Invoice> Invoices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
