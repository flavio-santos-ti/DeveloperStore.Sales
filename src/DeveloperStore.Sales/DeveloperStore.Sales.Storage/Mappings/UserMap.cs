using DeveloperStore.Sales.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Storage.Mappings;

[ExcludeFromCodeCoverage]
public class UserMap : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.Email)
               .HasColumnName("email")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(f => f.Username)
               .HasColumnName("username")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(f => f.PasswordHash)
               .HasColumnName("password_hash")
               .HasMaxLength(100);

        builder.Property(f => f.Firstname)
               .HasColumnName("firstname")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(f => f.Lastname)
               .HasColumnName("lastname")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(f => f.City)
               .HasColumnName("city")
               .HasMaxLength(100);

        builder.Property(f => f.Street)
               .HasColumnName("street")
               .HasMaxLength(255);

        builder.Property(f => f.AddressNumber)
               .HasColumnName("address_number");

        builder.Property(f => f.Zipcode)
               .HasColumnName("zipcode")
               .HasMaxLength(20);

        builder.Property(f => f.GeolocationLat)
               .HasColumnName("geolocation_lat")
               .HasMaxLength(50);

        builder.Property(f => f.GeolocationLong)
               .HasColumnName("geolocation_long")
               .HasMaxLength(50);

        builder.Property(f => f.Phone)
               .HasColumnName("phone")
               .HasMaxLength(20);

        builder.Property(f => f.Status)
               .HasColumnName("status")
               .HasMaxLength(20);

        builder.Property(f => f.Role)
               .HasColumnName("role")
               .HasMaxLength(20);
    }
}

