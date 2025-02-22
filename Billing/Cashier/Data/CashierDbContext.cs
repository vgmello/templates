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
        modelBuilder.ApplyConfiguration(new CashierEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CashierPaymentEntityConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is CashierEntity or CashierPaymentEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                ((dynamic)entry.Entity).CreatedDateUtc = DateTime.UtcNow;
            }
            ((dynamic)entry.Entity).UpdatedDateUtc = DateTime.UtcNow;
            ((dynamic)entry.Entity).Version++;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}