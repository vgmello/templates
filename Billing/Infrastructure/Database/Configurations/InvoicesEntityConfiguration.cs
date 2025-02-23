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
