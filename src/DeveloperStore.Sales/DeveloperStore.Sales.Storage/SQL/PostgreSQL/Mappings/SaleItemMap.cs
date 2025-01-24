using DeveloperStore.Sales.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Mappings;

[ExcludeFromCodeCoverage]
public class SaleItemMap : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");

        builder.HasKey(si => si.Id);
        builder.Property(si => si.Id).HasColumnName("id");

        builder.Property(si => si.SaleId)
               .HasColumnName("sale_id")
               .IsRequired();

        builder.Property(si => si.ProductId)
               .HasColumnName("product_id")
               .IsRequired();

        builder.Property(si => si.Quantity)
               .HasColumnName("quantity")
               .IsRequired();

        builder.Property(si => si.UnitPrice)
               .HasColumnName("unit_price")
               .HasColumnType("NUMERIC(10,2)")
               .IsRequired();

        builder.Property(si => si.Discount)
               .HasColumnName("discount")
               .HasColumnType("NUMERIC(10,2)");

        builder.Property(si => si.TotalAmount)
               .HasColumnName("total_amount")
               .HasColumnType("NUMERIC(10,2)")
               .IsRequired();
    }
}
