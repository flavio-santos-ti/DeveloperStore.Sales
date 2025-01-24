using DeveloperStore.Sales.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Mappings;

[ExcludeFromCodeCoverage]
public class CartMap : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.UserId)
               .HasColumnName("user_id")
               .IsRequired();

        builder.Property(c => c.Date)
               .HasColumnName("date")
               .IsRequired();

        builder.HasMany(c => c.CartProducts)
               .WithOne(cp => cp.Cart)
               .HasForeignKey(cp => cp.CartId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
