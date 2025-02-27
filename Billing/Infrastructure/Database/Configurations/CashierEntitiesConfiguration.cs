// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Database.Configurations;

public class CashierEntitiesConfiguration :
    IEntityTypeConfiguration<Cashier.Data.Entities.Cashier>,
    IEntityTypeConfiguration<CashierCurrency>
{
    public void Configure(EntityTypeBuilder<Cashier.Data.Entities.Cashier> builder)
    {
        builder.Property(e => e.CreatedDateUtc)
            .HasDefaultValueSql(DbConstants.CurrentTimeStamp)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UpdatedDateUtc)
            .HasDefaultValueSql(DbConstants.CurrentTimeStamp)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(1);
    }

    public void Configure(EntityTypeBuilder<CashierCurrency> builder)
    {
        builder.Property(e => e.CreatedDateUtc)
            .HasDefaultValueSql(DbConstants.CurrentTimeStamp);

        builder.HasOne(e => e.Cashier)
            .WithMany(e => e.Currencies)
            .HasForeignKey(e => e.CashierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
