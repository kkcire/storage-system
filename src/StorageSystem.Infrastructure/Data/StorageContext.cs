using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Infrastructure.Data;

public class StorageContext(DbContextOptions<StorageContext> options) : DbContext(options)
{
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(brand =>
        {
            brand.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(Brand.MaxNameLength);

            brand.HasIndex(b => b.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(product =>
        {
            product.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(Brand.MaxNameLength);

            product.Property(p => p.Price)
                .HasPrecision(18, 2);

            product.HasIndex(p => p.Name).IsUnique();

            product.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}