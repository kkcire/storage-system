using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Data;

public class StorageContext(DbContextOptions<StorageContext> options) : DbContext(options)
{
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }
}