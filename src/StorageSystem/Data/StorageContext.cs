using Microsoft.EntityFrameworkCore;
using StorageSystem.Entities;

namespace StorageSystem.Data;

public class StorageContext : DbContext
{
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source = storage.db");
    }
}