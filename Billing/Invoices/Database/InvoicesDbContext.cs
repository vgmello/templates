using Billing.Invoices.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Invoices.Database;

public class InvoicesDbContext(DbContextOptions<InvoicesDbContext> options) : DbContext(options)
{
    public DbSet<InvoiceEntity> Invoices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoicesDbContext).Assembly);
    }
}
