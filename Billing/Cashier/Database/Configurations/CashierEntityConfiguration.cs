using Billing.Cashier.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Cashier.Database.Configurations;

public class CashierEntityConfiguration : IEntityTypeConfiguration<CashierEntity>
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

public class CashierPaymentEntityConfiguration : IEntityTypeConfiguration<CashierPaymentEntity>
{
    public void Configure(EntityTypeBuilder<CashierPaymentEntity> builder)
    {
        builder.Property(e => e.CreatedDateUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedDateUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(1);

        // builder.HasOne(e => e.Cashier)
        //     .WithMany(e => e.CashierPayments)
        //     .HasForeignKey(e => e.CashierId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
