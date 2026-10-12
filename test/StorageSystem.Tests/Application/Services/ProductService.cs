using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Application.Repository;
using StorageSystem.Application.Services;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Exceptions;
using StorageSystem.Infrastructure.Data;
using StorageSystem.Infrastructure.Repository;

namespace StorageSystem.Tests.Application;

public class ProductServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly StorageContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new StorageContext(options);
        _context.Database.EnsureCreated();

        IProductRepository productRepository = new ProductRepository(_context);
        _service = new ProductService(productRepository);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<Brand> CreateBrandAsync(string name = "AMD")
    {
        var brand = Brand.Create(name);
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();
        return brand;
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_WhenInputsAreValid_ShouldPersistAndReturnProduct()
    {
        var brand = await CreateBrandAsync();

        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        Assert.NotEqual(0, product.Id);
        Assert.Equal("Ryzen 7", product.Name);
        Assert.Equal(2899.90m, product.Price);
        Assert.Equal(10, product.Quantity);
        Assert.Equal(brand.Id, product.BrandId);

        var saved = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        Assert.NotNull(saved);
        Assert.Equal("Ryzen 7", saved.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenBrandDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateAsync("Ryzen 7", 2899.90m, 10, brandId: 999, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("Ryzen 7", 1000m, 5, brand.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByWhitespace_ShouldThrowDomainException()
    {
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("  Ryzen 7  ", 2899.90m, 10, brand.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByCase_ShouldThrowDomainException()
    {
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);
        
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None));
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_WhenInputsAreValid_ShouldPersistAllFields()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await _service.UpdateAsync(product.Id, "Ryzen 9", 3999.90m, 5, CancellationToken.None);

        var reloaded = await _context.Products
            .AsNoTracking()
            .FirstAsync(p => p.Id == product.Id);

        Assert.Equal("Ryzen 9", reloaded.Name);
        Assert.Equal(3999.90m, reloaded.Price);
        Assert.Equal(5, reloaded.Quantity);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, "Ryzen 9", 3999.90m, 5, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameConflictsWithAnotherProduct_ShouldThrowDomainException()
    {
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);
        var second = await _service.CreateAsync("Ryzen 5", 1500m, 10, brand.Id, CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.UpdateAsync(second.Id, "Ryzen 7", 1500m, 10, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenRenamingToSameName_ShouldNotThrow()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        var updated = await _service.UpdateAsync(product.Id, "Ryzen 7", 3000m, 10, CancellationToken.None);

        Assert.Equal("Ryzen 7", updated.Name);
        Assert.Equal(3000m, updated.Price);
    }

    // ---------- PatchAsync ----------

    [Fact]
    public async Task PatchAsync_WhenOnlyNameIsProvided_ShouldUpdateOnlyName()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await _service.PatchAsync(product.Id, newName: "Ryzen 9", cancellationToken: CancellationToken.None);

        var reloaded = await _context.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        Assert.Equal("Ryzen 9", reloaded.Name);
        Assert.Equal(2899.90m, reloaded.Price);
        Assert.Equal(10, reloaded.Quantity);
    }

    [Fact]
    public async Task PatchAsync_WhenOnlyPriceIsProvided_ShouldUpdateOnlyPrice()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await _service.PatchAsync(product.Id, newPrice: 3999.90m, cancellationToken: CancellationToken.None);

        var reloaded = await _context.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        Assert.Equal("Ryzen 7", reloaded.Name);
        Assert.Equal(3999.90m, reloaded.Price);
        Assert.Equal(10, reloaded.Quantity);
    }

    [Fact]
    public async Task PatchAsync_WhenOnlyQuantityIsProvided_ShouldUpdateOnlyQuantity()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await _service.PatchAsync(product.Id, newQuantity: 20, cancellationToken: CancellationToken.None);

        var reloaded = await _context.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        Assert.Equal("Ryzen 7", reloaded.Name);
        Assert.Equal(2899.90m, reloaded.Price);
        Assert.Equal(20, reloaded.Quantity);
    }

    [Fact]
    public async Task PatchAsync_WhenAllFieldsAreNull_ShouldKeepProductUnchanged()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.PatchAsync(product.Id, cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task PatchAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.PatchAsync(999, newName: "Ryzen 9", cancellationToken: CancellationToken.None));
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ShouldRemove()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await _service.DeleteAsync(product.Id, CancellationToken.None);

        var deleted = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(999, CancellationToken.None));
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnProduct()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        var found = await _service.GetByIdAsync(product.Id, CancellationToken.None);

        Assert.Equal(product.Id, found.Id);
        Assert.Equal("Ryzen 7", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(999, CancellationToken.None));
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ShouldReturnAll()
    {
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);
        await _service.CreateAsync("Ryzen 5", 1500m, 10, brand.Id, CancellationToken.None);

        var all = await _service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProducts_ShouldReturnEmptyList()
    {
        var all = await _service.GetAllAsync(CancellationToken.None);

        Assert.Empty(all);
    }

    // ---------- SearchByNameAsync ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchByNameAsync_WhenNameIsNullOrWhiteSpace_ShouldThrowDomainException(string? name)
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _service.SearchByNameAsync(name!, CancellationToken.None));
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNameIsPartial_ShouldReturnMatches()
    {
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7 7800X3D", 2899.90m, 10, brand.Id, CancellationToken.None);
        await _service.CreateAsync("Ryzen 5 5600", 1200m, 10, brand.Id, CancellationToken.None);
        await _service.CreateAsync("Intel Core i7", 2500m, 10, brand.Id, CancellationToken.None);

        var results = await _service.SearchByNameAsync("ryzen", CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Contains("Ryzen", p.Name));
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        var results = await _service.SearchByNameAsync("Intel", CancellationToken.None);

        Assert.Empty(results);
    }

    // ---------- AddStockAsync ----------

    [Fact]
    public async Task AddStockAsync_WhenAmountIsPositive_ShouldIncreaseQuantity()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        var updated = await _service.AddStockAsync(product.Id, 5, CancellationToken.None);

        Assert.Equal(15, updated.Quantity);
    }

    [Fact]
    public async Task AddStockAsync_WhenAmountIsNegative_ShouldDecreaseQuantity()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        var updated = await _service.AddStockAsync(product.Id, -3, CancellationToken.None);

        Assert.Equal(7, updated.Quantity);
    }

    [Fact]
    public async Task AddStockAsync_WhenWouldGoNegative_ShouldThrowDomainException()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.AddStockAsync(product.Id, -20, CancellationToken.None));
    }

    [Fact]
    public async Task AddStockAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddStockAsync(999, 5, CancellationToken.None));
    }

    // ---------- ChangeBrandAsync ----------

    [Fact]
    public async Task ChangeBrandAsync_WhenBrandExists_ShouldChange()
    {
        var amd = await CreateBrandAsync("AMD");
        var intel = await CreateBrandAsync("Intel");
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, amd.Id, CancellationToken.None);

        var updated = await _service.ChangeBrandAsync(product.Id, intel.Id, CancellationToken.None);

        Assert.Equal(intel.Id, updated.BrandId);
    }

    [Fact]
    public async Task ChangeBrandAsync_WhenBrandDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ChangeBrandAsync(product.Id, 999, CancellationToken.None));
    }

    [Fact]
    public async Task ChangeBrandAsync_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var brand = await CreateBrandAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ChangeBrandAsync(999, brand.Id, CancellationToken.None));
    }
}