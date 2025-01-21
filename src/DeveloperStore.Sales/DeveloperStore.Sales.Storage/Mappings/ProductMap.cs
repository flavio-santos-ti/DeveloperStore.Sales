using DeveloperStore.Sales.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
namespace DeveloperStore.Sales.Storage.Mappings;

[ExcludeFromCodeCoverage]
public class ProductMap : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.Title)
               .HasColumnName("title")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasColumnType("NUMERIC(10,2)")
            .IsRequired();

        builder.Property(f => f.Description)
               .HasColumnName("description")
               .IsRequired();

        builder.Property(f => f.Category)
               .HasColumnName("category")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(f => f.Image)
               .HasColumnName("image")
               .HasMaxLength(255)
               .IsRequired();

        builder.OwnsOne(p => p.Rating, rating =>
        {
            rating.Property(r => r.Rate).HasColumnName("rating_rate");
            rating.Property(r => r.Count).HasColumnName("rating_count");
        });
    }
}
