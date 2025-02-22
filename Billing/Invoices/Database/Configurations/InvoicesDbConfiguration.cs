using Billing.Cashier.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Invoices.Database.Configurations;

public class InvoicesDbConfiguration : IEntityTypeConfiguration<CashierEntity>
{
    public void Configure(EntityTypeBuilder<CashierEntity> builder)
    {
        builder.Property(e => e.CreatedDateUtc)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UpdatedDateUtc)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(1);
    }
}
