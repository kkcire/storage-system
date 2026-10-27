namespace StorageSystem.Tests.Application;

[Trait("Category", "Integration")]
public class BrandServiceTests : IDisposable
{
    private readonly SqlServerTestDatabase _database;
    private readonly StorageContext _context;
    private readonly BrandService _service;

    public BrandServiceTests()
    {
        _database = new SqlServerTestDatabase();
        _context = _database.CreateContext();

        IBrandRepository brandRepository = new BrandRepository(_context);
        _service = new BrandService(brandRepository);
    }

    public void Dispose()
    {
        _context.Dispose();
        _database.Dispose();
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_WhenNameIsValid_ShouldReturnBrandWithGeneratedId()
    {
        // Act
        var brand = await _service.CreateAsync("Test Brand", CancellationToken.None);

        // Assert
        Assert.NotEqual(0, brand.Id);
        Assert.Equal("Test Brand", brand.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsValid_ShouldPersistBrandInDatabase()
    {
        // Arrange
        var brand = await _service.CreateAsync("Test Brand", CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Act
        var saved = await _context.Brands.FindAsync(brand.Id);

        // Assert
        Assert.NotNull(saved);
        Assert.Equal("Test Brand", saved.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        await _service.CreateAsync("AMD", CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("AMD", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        await _service.CreateAsync("AMD", CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("  AMD  ", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByCase_ShouldThrowDomainException()
    {
        // Arrange
        await _service.CreateAsync("AMD", CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("amd", CancellationToken.None));
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_WhenNameIsValid_ShouldPersistNewName()
    {
        // Arrange
        var brand = await _service.CreateAsync("Old Name", CancellationToken.None);

        // Act
        await _service.UpdateAsync(brand.Id, "New Name", CancellationToken.None);

        // Assert
        _context.ChangeTracker.Clear();
        var reloaded = await _context.Brands.FindAsync(brand.Id);
        Assert.Equal("New Name", reloaded!.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameHasWhitespace_ShouldPersistTrimmed()
    {
        // Arrange
        var brand = await _service.CreateAsync("Old Name", CancellationToken.None);

        // Act
        await _service.UpdateAsync(brand.Id, "  Intel  ", CancellationToken.None);

        // Assert
        _context.ChangeTracker.Clear();
        var reloaded = await _context.Brands.FindAsync(brand.Id);
        Assert.Equal("Intel", reloaded!.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, "New Name", CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameExistsInAnotherBrand_ShouldThrowDomainException()
    {
        // Arrange
        await _service.CreateAsync("AMD", CancellationToken.None);
        var intel = await _service.CreateAsync("Intel", CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.UpdateAsync(intel.Id, "AMD", CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenRenamingToSameName_ShouldNotThrow()
    {
        // Arrange
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);

        // Act
        var updated = await _service.UpdateAsync(brand.Id, "AMD", CancellationToken.None);

        // Assert
        Assert.Equal("AMD", updated.Name);
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_WhenBrandHasNoProducts_ShouldRemove()
    {
        // Arrange
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);

        // Act
        await _service.DeleteAsync(brand.Id, CancellationToken.None);

        // Assert
        _context.ChangeTracker.Clear();
        var deleted = await _context.Brands.FindAsync(brand.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenBrandHasProducts_ShouldThrowDomainException()
    {
        // Arrange
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);
        var product = Product.Create("Ryzen", 2899.90m, 10, brand.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.DeleteAsync(brand.Id, CancellationToken.None));
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
    public async Task GetByIdAsync_WhenExists_ShouldReturnBrand()
    {
        // Arrange
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);

        // Act
        var found = await _service.GetByIdAsync(brand.Id, CancellationToken.None);

        // Assert
        Assert.Equal(brand.Id, found.Id);
        Assert.Equal("AMD", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(999, CancellationToken.None));
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
        await _service.CreateAsync("AMD", CancellationToken.None);
        await _service.CreateAsync("Intel", CancellationToken.None);
        await _service.CreateAsync("NVIDIA", CancellationToken.None);

        // Act
        var results = await _service.SearchByNameAsync("d", CancellationToken.None);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, b => b.Name == "AMD");
        Assert.Contains(results, b => b.Name == "NVIDIA");
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        await _service.CreateAsync("AMD", CancellationToken.None);

        // Act
        var results = await _service.SearchByNameAsync("Intel", CancellationToken.None);

        // Assert
        Assert.Empty(results);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_WhenBrandsExist_ShouldReturnAll()
    {
        // Arrange
        await _service.CreateAsync("AMD", CancellationToken.None);
        await _service.CreateAsync("Intel", CancellationToken.None);

        // Act
        var all = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBrands_ShouldReturnEmptyList()
    {
        // Act
        var all = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Empty(all);
    }
}