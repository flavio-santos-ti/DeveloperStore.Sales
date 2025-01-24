using DeveloperStore.Sales.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Mappings;

[ExcludeFromCodeCoverage]
public class SaleMap : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.SaleNumber)
               .HasColumnName("sale_number")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(s => s.SaleDate)
               .HasColumnName("sale_date")
               .IsRequired();

        builder.Property(s => s.CustomerId)
               .HasColumnName("customer_id")
               .IsRequired();

        builder.Property(s => s.Branch)
               .HasColumnName("branch")
               .HasMaxLength(100);

        builder.Property(s => s.TotalAmount)
               .HasColumnName("total_amount")
               .HasColumnType("NUMERIC(10,2)")
               .IsRequired();

        builder.Property(s => s.IsCancelled)
               .HasColumnName("is_cancelled")
               .IsRequired();

        builder.HasMany(s => s.Items)
               .WithOne()
               .HasForeignKey(i => i.SaleId);
    }
}
