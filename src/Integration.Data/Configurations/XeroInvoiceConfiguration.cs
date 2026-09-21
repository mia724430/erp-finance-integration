using Integration.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Integration.Data.Configurations;

/// <summary>
/// XeroInvoice is a plain business model in Integration.Core with no persistence
/// concerns of its own; the database-only surrogate key and column constraints
/// are configured here instead so Integration.Core stays free of any EF Core
/// dependency.
/// </summary>
public class XeroInvoiceConfiguration : IEntityTypeConfiguration<XeroInvoice>
{
    public void Configure(EntityTypeBuilder<XeroInvoice> builder)
    {
        builder.ToTable("XeroInvoices");

        builder.Property<int>("Id").ValueGeneratedOnAdd();
        builder.HasKey("Id");

        builder.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");

        // Backstop, not the primary defense: Worker.cs checks for existing invoice
        // numbers before inserting (see InvoiceDeduplicator), so this should only
        // ever fire if that check races with another writer.
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
    }
}
