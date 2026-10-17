using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Entities;
using StorageSystem.Infrastructure.Data;

namespace StorageSystem.Tests.Infrastructure;

[Trait("Category", "Integration")]
public class ConstraintsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly StorageContext _context;

    public ConstraintsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new StorageContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    // ---------- Unique index: Brand.Name ----------

    [Fact]
    public async Task SaveChanges_WhenTwoBrandsWithSameName_ShouldThrowDbUpdateException()
    {
        // Arrange
        _context.Brands.Add(Brand.Create("AMD"));
        _context.Brands.Add(Brand.Create("AMD"));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    // ---------- Unique index: Product.Name ----------

    [Fact]
    public async Task SaveChanges_WhenTwoProductsWithSameName_ShouldThrowDbUpdateException()
    {
        // Arrange
        var brand = Brand.Create("AMD");
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        _context.Products.Add(Product.Create("Ryzen 7", 1500m, 5, brand.Id));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    // ---------- Restrict: Brand com Products ----------

    [Fact]
    public async Task SaveChanges_WhenRemovingBrandWithProducts_ShouldThrowDbUpdateException()
    {
        // Arrange
        var brand = Brand.Create("AMD");
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var brandToDelete = await _context.Brands.FindAsync(brand.Id);

        // Act
        _context.Brands.Remove(brandToDelete!);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenRemovingBrandWithoutProducts_ShouldSucceed()
    {
        // Arrange
        var brand = Brand.Create("AMD");
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        // Act
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();

        // Assert
        _context.ChangeTracker.Clear();
        var deleted = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == brand.Id);
        Assert.Null(deleted);
    }
}