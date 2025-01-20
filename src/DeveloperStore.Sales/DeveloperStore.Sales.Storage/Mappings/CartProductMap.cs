using DeveloperStore.Sales.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperStore.Sales.Storage.Mappings;

public class CartProductMap : IEntityTypeConfiguration<CartProduct>
{
    public void Configure(EntityTypeBuilder<CartProduct> builder)
    {
        builder.ToTable("cart_products");

        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Id).HasColumnName("id");

        builder.Property(cp => cp.CartId)
               .HasColumnName("cart_id")
               .IsRequired();

        builder.Property(cp => cp.ProductId)
               .HasColumnName("product_id")
               .IsRequired();

        builder.Property(cp => cp.Quantity)
               .HasColumnName("quantity")
               .IsRequired();

        builder.HasOne(cp => cp.Cart)
               .WithMany(c => c.CartProducts)
               .HasForeignKey(cp => cp.CartId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Product)
               .WithMany()
               .HasForeignKey(cp => cp.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
