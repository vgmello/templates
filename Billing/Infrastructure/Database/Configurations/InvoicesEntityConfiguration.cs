// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Database.Configurations;

public class InvoicesEntityConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(e => e.CreatedDateUtc)
            .HasDefaultValueSql(DbContants.CurrentTimeStamp)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UpdatedDateUtc)
            .HasDefaultValueSql(DbContants.CurrentTimeStamp)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(1);
    }
}
