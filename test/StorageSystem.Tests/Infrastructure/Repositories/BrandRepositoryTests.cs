using StorageSystem.Tests.Infrastructure;

namespace StorageSystem.Tests.Infrastructure.Repositories;

[Trait("Category", "Integration")]
public class BrandRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database;
    private readonly StorageContext _context;
    private readonly BrandRepository _repository;

    public BrandRepositoryTests()
    {
        _database = new SqliteTestDatabase();
        _context = _database.CreateContext();

        _repository = new BrandRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _database.Dispose();
    }

    private async Task<Brand> AddBrandAsync(string name)
    {
        var brand = Brand.Create(name);
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();
        return brand;
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnBrand()
    {
        // Arrange
        var brand = await AddBrandAsync("AMD");
        _context.ChangeTracker.Clear();

        // Act
        var found = await _repository.GetByIdAsync(brand.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(found);
        Assert.Equal("AMD", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldReturnNull()
    {
        // Act
        var found = await _repository.GetByIdAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldTrackEntity()
    {
        // Arrange
        var brand = await AddBrandAsync("AMD");
        _context.ChangeTracker.Clear();

        // Act
        var found = await _repository.GetByIdAsync(brand.Id, CancellationToken.None);

        // Assert
        Assert.Equal(EntityState.Unchanged, _context.Entry(found!).State);

        found!.Rename("Intel");
        await _repository.SaveChangesAsync(CancellationToken.None);

        _context.ChangeTracker.Clear();
        var reloaded = await _context.Brands.AsNoTracking().FirstAsync(b => b.Id == brand.Id);
        Assert.Equal("Intel", reloaded.Name);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_WhenBrandsExist_ShouldReturnAll()
    {
        // Arrange
        await AddBrandAsync("AMD");
        await AddBrandAsync("Intel");
        await AddBrandAsync("NVIDIA");

        // Act
        var brands = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, brands.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBrands_ShouldReturnEmptyList()
    {
        // Act
        var brands = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Empty(brands);
    }

    // ---------- SearchByNameAsync ----------

    [Fact]
    public async Task SearchByNameAsync_WhenPartialMatch_ShouldReturnMatches()
    {
        // Arrange
        await AddBrandAsync("AMD");
        await AddBrandAsync("Amdahl");
        await AddBrandAsync("Intel");

        // Act
        var results = await _repository.SearchByNameAsync("md", CancellationToken.None);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, b => b.Name == "AMD");
        Assert.Contains(results, b => b.Name == "Amdahl");
    }

    [Fact]
    public async Task SearchByNameAsync_WhenCaseDiffers_ShouldMatch()
    {
        // Arrange
        await AddBrandAsync("AMD");

        // Act
        var results = await _repository.SearchByNameAsync("amd", CancellationToken.None);

        // Assert
        Assert.Single(results);
        Assert.Equal("AMD", results[0].Name);
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        await AddBrandAsync("AMD");

        // Act
        var results = await _repository.SearchByNameAsync("Intel", CancellationToken.None);

        // Assert
        Assert.Empty(results);
    }

    // ---------- ExistsBrandByNameAsync ----------

    [Fact]
    public async Task ExistsBrandByNameAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        await AddBrandAsync("AMD");

        // Act
        var exists = await _repository.ExistsBrandByNameAsync("AMD", CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsBrandByNameAsync_WhenCaseDiffers_ShouldReturnTrue()
    {
        // Arrange
        await AddBrandAsync("AMD");

        // Act
        var exists = await _repository.ExistsBrandByNameAsync("amd", CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsBrandByNameAsync_WhenDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.ExistsBrandByNameAsync("AMD", CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsBrandByNameAsync_WithIgnoreId_ShouldIgnoreThatBrand()
    {
        // Arrange
        var brand = await AddBrandAsync("AMD");

        // Act - "AMD" exists, but is the same brand
        var exists = await _repository.ExistsBrandByNameAsync("AMD", ignoreId: brand.Id, CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsBrandByNameAsync_WithIgnoreId_WhenAnotherBrandHasName_ShouldReturnTrue()
    {
        // Arrange
        await AddBrandAsync("AMD");
        var second = await AddBrandAsync("Intel");

        // Act
        var exists = await _repository.ExistsBrandByNameAsync("AMD", ignoreId: second.Id, CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    // ---------- HasProductsAsync ----------

    [Fact]
    public async Task HasProductsAsync_WhenBrandHasProducts_ShouldReturnTrue()
    {
        // Arrange
        var brand = await AddBrandAsync("AMD");
        var product = Product.Create("Ryzen 7", 2899.90m, 10, brand.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var hasProducts = await _repository.HasProductsAsync(brand.Id, CancellationToken.None);

        // Assert
        Assert.True(hasProducts);
    }

    [Fact]
    public async Task HasProductsAsync_WhenBrandHasNoProducts_ShouldReturnFalse()
    {
        // Arrange
        var brand = await AddBrandAsync("AMD");

        // Act
        var hasProducts = await _repository.HasProductsAsync(brand.Id, CancellationToken.None);

        // Assert
        Assert.False(hasProducts);
    }

    [Fact]
    public async Task HasProductsAsync_WhenBrandDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var hasProducts = await _repository.HasProductsAsync(999, CancellationToken.None);

        // Assert
        Assert.False(hasProducts);
    }
}