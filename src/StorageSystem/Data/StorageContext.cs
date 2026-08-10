using Microsoft.EntityFrameworkCore;
using StorageSystem.Entities;

namespace StorageSystem.Data;

public class StorageContext(DbContextOptions<StorageContext> options) : DbContext(options)
{
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }
}