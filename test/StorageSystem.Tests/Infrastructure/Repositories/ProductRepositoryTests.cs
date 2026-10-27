namespace StorageSystem.Tests.Infrastructure;

[Trait("Category", "Integration")]
public class ProductRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database;
    private readonly StorageContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _database = new SqliteTestDatabase();
        _context = _database.CreateContext();

        _repository = new ProductRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _database.Dispose();
    }

    private async Task<Brand> CreateBrandAsync(string name = "AMD")
    {
        var brand = Brand.Create(name);
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();
        return brand;
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeBrand()
    {
        // Arrange
        var brand = await CreateBrandAsync("AMD");
        var product = Product.Create("Ryzen 7", 2899.90m, 10, brand.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var found = await _repository.GetByIdAsync(product.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(found);
        Assert.NotNull(found.Brand);
        Assert.Equal("AMD", found.Brand.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldReturnNull()
    {
        // Act
        var found = await _repository.GetByIdAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(found);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_ShouldIncludeBrand()
    {
        // Arrange
        var brand = await CreateBrandAsync("AMD");
        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var products = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Single(products);
        Assert.NotNull(products[0].Brand);
        Assert.Equal("AMD", products[0].Brand.Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProducts_ShouldReturnEmptyList()
    {
        // Act
        var products = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Empty(products);
    }

    // ---------- SearchByNameAsync ----------

    [Fact]
    public async Task SearchByNameAsync_WhenPartialMatch_ShouldReturnMatches()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        _context.Products.Add(Product.Create("Ryzen 7 7800X3D", 2899.90m, 10, brand.Id));
        _context.Products.Add(Product.Create("Ryzen 5 5600", 1200m, 10, brand.Id));
        _context.Products.Add(Product.Create("Intel Core i7", 2500m, 10, brand.Id));
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.SearchByNameAsync("ryzen", CancellationToken.None);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Contains("Ryzen", p.Name));
    }

    [Fact]
    public async Task SearchByNameAsync_WhenCaseDiffers_ShouldMatch()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.SearchByNameAsync("ryzen 7", CancellationToken.None);

        // Assert
        Assert.Single(results);
        Assert.Equal("Ryzen 7", results[0].Name);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldIncludeBrand()
    {
        // Arrange
        var brand = await CreateBrandAsync("AMD");
        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var results = await _repository.SearchByNameAsync("ryzen", CancellationToken.None);

        // Assert
        Assert.NotNull(results[0].Brand);
        Assert.Equal("AMD", results[0].Brand.Name);
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.SearchByNameAsync("Intel", CancellationToken.None);

        // Assert
        Assert.Empty(results);
    }

    // ---------- ExistsProductByNameAsync ----------

    [Fact]
    public async Task ExistsProductByNameAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsProductByNameAsync("Ryzen 7", CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsProductByNameAsync_WhenCaseDiffers_ShouldReturnTrue()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsProductByNameAsync("ryzen 7", CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsProductByNameAsync_WhenDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.ExistsProductByNameAsync("Ryzen 7", CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsProductByNameAsync_WithIgnoreId_ShouldIgnoreThatProduct()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = Product.Create("Ryzen 7", 2899.90m, 10, brand.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act - "Ryzen 7" exists, but is the same product
        var exists = await _repository.ExistsProductByNameAsync("Ryzen 7", ignoreId: product.Id, CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsProductByNameAsync_WithIgnoreId_WhenAnotherProductHasName_ShouldReturnTrue()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        _context.Products.Add(Product.Create("Ryzen 7", 2899.90m, 10, brand.Id));
        var second = Product.Create("Ryzen 5", 1200m, 10, brand.Id);
        _context.Products.Add(second);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsProductByNameAsync("Ryzen 7", ignoreId: second.Id, CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    // ---------- BrandExistsAsync ----------

    [Fact]
    public async Task BrandExistsAsync_WhenBrandExists_ShouldReturnTrue()
    {
        // Arrange
        var brand = await CreateBrandAsync();

        // Act
        var exists = await _repository.BrandExistsAsync(brand.Id, CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task BrandExistsAsync_WhenBrandDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.BrandExistsAsync(999, CancellationToken.None);

        // Assert
        Assert.False(exists);
    }
}