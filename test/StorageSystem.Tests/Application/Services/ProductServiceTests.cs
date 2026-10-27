namespace StorageSystem.Tests.Application;

[Trait("Category", "Integration")]
public class ProductServiceTests : IDisposable
{
    private readonly SqlServerTestDatabase _database;
    private readonly StorageContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _database = new SqlServerTestDatabase();
        _context = _database.CreateContext();

        IProductRepository productRepository = new ProductRepository(_context);
        _service = new ProductService(productRepository);
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

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_WhenInputsAreValid_ShouldPersistAndReturnProduct()
    {
        // Arrange
        var brand = await CreateBrandAsync();

        // Act
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Assert
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
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateAsync("Ryzen 7", 2899.90m, 10, brandId: 999, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("Ryzen 7", 1000m, 5, brand.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("  Ryzen 7  ", 2899.90m, 10, brand.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByCase_ShouldThrowDomainException()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None));
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_WhenInputsAreValid_ShouldPersistAllFields()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        await _service.UpdateAsync(product.Id, "Ryzen 9", 3999.90m, 5, CancellationToken.None);

        // Assert
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
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, "Ryzen 9", 3999.90m, 5, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameConflictsWithAnotherProduct_ShouldThrowDomainException()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);
        var second = await _service.CreateAsync("Ryzen 5", 1500m, 10, brand.Id, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.UpdateAsync(second.Id, "Ryzen 7", 1500m, 10, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenRenamingToSameName_ShouldNotThrow()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        var updated = await _service.UpdateAsync(product.Id, "Ryzen 7", 3000m, 10, CancellationToken.None);

        // Assert
        Assert.Equal("Ryzen 7", updated.Name);
        Assert.Equal(3000m, updated.Price);
    }

    // ---------- PatchAsync ----------

    [Fact]
    public async Task PatchAsync_WhenOnlyNameIsProvided_ShouldUpdateOnlyName()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        await _service.PatchAsync(product.Id, newName: "Ryzen 9", cancellationToken: CancellationToken.None);

        // Assert
        var reloaded = await _context.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        Assert.Equal("Ryzen 9", reloaded.Name);
        Assert.Equal(2899.90m, reloaded.Price);
        Assert.Equal(10, reloaded.Quantity);
    }

    [Fact]
    public async Task PatchAsync_WhenOnlyPriceIsProvided_ShouldUpdateOnlyPrice()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        await _service.PatchAsync(product.Id, newPrice: 3999.90m, cancellationToken: CancellationToken.None);

        // Assert
        var reloaded = await _context.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        Assert.Equal("Ryzen 7", reloaded.Name);
        Assert.Equal(3999.90m, reloaded.Price);
        Assert.Equal(10, reloaded.Quantity);
    }

    [Fact]
    public async Task PatchAsync_WhenOnlyQuantityIsProvided_ShouldUpdateOnlyQuantity()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        await _service.PatchAsync(product.Id, newQuantity: 20, cancellationToken: CancellationToken.None);

        // Assert
        var reloaded = await _context.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        Assert.Equal("Ryzen 7", reloaded.Name);
        Assert.Equal(2899.90m, reloaded.Price);
        Assert.Equal(20, reloaded.Quantity);
    }

    [Fact]
    public async Task PatchAsync_WhenAllFieldsAreNull_ShouldThrowDomainException()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.PatchAsync(product.Id, cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task PatchAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.PatchAsync(999, newName: "Ryzen 9", cancellationToken: CancellationToken.None));
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ShouldRemove()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        await _service.DeleteAsync(product.Id, CancellationToken.None);

        // Assert
        var deleted = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(999, CancellationToken.None));
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnProduct()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        var found = await _service.GetByIdAsync(product.Id, CancellationToken.None);

        // Assert
        Assert.Equal(product.Id, found.Id);
        Assert.Equal("Ryzen 7", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(999, CancellationToken.None));
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ShouldReturnAll()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);
        await _service.CreateAsync("Ryzen 5", 1500m, 10, brand.Id, CancellationToken.None);

        // Act
        var all = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProducts_ShouldReturnEmptyList()
    {
        // Act
        var all = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Empty(all);
    }

    // ---------- SearchByNameAsync ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchByNameAsync_WhenNameIsNullOrWhiteSpace_ShouldThrowDomainException(string? name)
    {
        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.SearchByNameAsync(name!, CancellationToken.None));
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNameIsPartial_ShouldReturnMatches()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7 7800X3D", 2899.90m, 10, brand.Id, CancellationToken.None);
        await _service.CreateAsync("Ryzen 5 5600", 1200m, 10, brand.Id, CancellationToken.None);
        await _service.CreateAsync("Intel Core i7", 2500m, 10, brand.Id, CancellationToken.None);

        // Act
        var results = await _service.SearchByNameAsync("ryzen", CancellationToken.None);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Contains("Ryzen", p.Name));
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        var results = await _service.SearchByNameAsync("Intel", CancellationToken.None);

        // Assert
        Assert.Empty(results);
    }

    // ---------- AddStockAsync ----------

    [Fact]
    public async Task AddStockAsync_WhenAmountIsPositive_ShouldIncreaseQuantity()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        var updated = await _service.AddStockAsync(product.Id, 5, CancellationToken.None);

        // Assert
        Assert.Equal(15, updated.Quantity);
    }

    [Fact]
    public async Task AddStockAsync_WhenAmountIsNegative_ShouldDecreaseQuantity()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act
        var updated = await _service.AddStockAsync(product.Id, -3, CancellationToken.None);

        // Assert
        Assert.Equal(7, updated.Quantity);
    }

    [Fact]
    public async Task AddStockAsync_WhenWouldGoNegative_ShouldThrowDomainException()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.AddStockAsync(product.Id, -20, CancellationToken.None));
    }

    [Fact]
    public async Task AddStockAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddStockAsync(999, 5, CancellationToken.None));
    }

    // ---------- ChangeBrandAsync ----------

    [Fact]
    public async Task ChangeBrandAsync_WhenBrandExists_ShouldChange()
    {
        // Arrange
        var amd = await CreateBrandAsync("AMD");
        var intel = await CreateBrandAsync("Intel");
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, amd.Id, CancellationToken.None);

        // Act
        var updated = await _service.ChangeBrandAsync(product.Id, intel.Id, CancellationToken.None);

        // Assert
        Assert.Equal(intel.Id, updated.BrandId);
    }

    [Fact]
    public async Task ChangeBrandAsync_WhenBrandDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var brand = await CreateBrandAsync();
        var product = await _service.CreateAsync("Ryzen 7", 2899.90m, 10, brand.Id, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ChangeBrandAsync(product.Id, 999, CancellationToken.None));
    }

    [Fact]
    public async Task ChangeBrandAsync_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var brand = await CreateBrandAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ChangeBrandAsync(999, brand.Id, CancellationToken.None));
    }
}