namespace StorageSystem.Tests.Application;

public class BrandServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly StorageContext _context;
    private readonly BrandService _service;

    public BrandServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new StorageContext(options);
        _context.Database.EnsureCreated();

        IBrandRepository brandRepository = new BrandRepository(_context);
        _service = new BrandService(brandRepository);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsValid_ShouldReturnBrandWithGeneratedId()
    {
        var brand = await _service.CreateAsync("Test Brand", CancellationToken.None);

        Assert.NotEqual(0, brand.Id);
        Assert.Equal("Test Brand", brand.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsValid_ShouldPersistBrandInDatabase()
    {
        var brand = await _service.CreateAsync("Test Brand", CancellationToken.None);

        _context.ChangeTracker.Clear();
        var saved = await _context.Brands.FindAsync(brand.Id);

        Assert.NotNull(saved);
        Assert.Equal("Test Brand", saved.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        await _service.CreateAsync("AMD", CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("AMD", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByWhitespace_ShouldThrowDomainException()
    {
        await _service.CreateAsync("AMD", CancellationToken.None);

        // "  AMD  " vira "AMD" no Create; a unicidade precisa pegar
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("  AMD  ", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNameDiffersOnlyByCase_ShouldThrowDomainException()
    {
        await _service.CreateAsync("AMD", CancellationToken.None);

        // Regra: unicidade é case-insensitive (ToLower nos dois lados).
        // Se remover o ToLower() do repositório, este teste quebra — intencional.
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync("amd", CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenNameIsValid_ShouldPersistNewName()
    {
        var brand = await _service.CreateAsync("Old Name", CancellationToken.None);

        await _service.UpdateAsync(brand.Id, "New Name", CancellationToken.None);

        _context.ChangeTracker.Clear();
        var reloaded = await _context.Brands.FindAsync(brand.Id);
        Assert.Equal("New Name", reloaded!.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameHasWhitespace_ShouldPersistTrimmed()
    {
        var brand = await _service.CreateAsync("Old Name", CancellationToken.None);

        await _service.UpdateAsync(brand.Id, "  Intel  ", CancellationToken.None);

        _context.ChangeTracker.Clear();
        var reloaded = await _context.Brands.FindAsync(brand.Id);
        Assert.Equal("Intel", reloaded!.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, "New Name", CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameExistsInAnotherBrand_ShouldThrowDomainException()
    {
        await _service.CreateAsync("AMD", CancellationToken.None);
        var intel = await _service.CreateAsync("Intel", CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.UpdateAsync(intel.Id, "AMD", CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenRenamingToSameName_ShouldNotThrow()
    {
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);

        var updated = await _service.UpdateAsync(brand.Id, "AMD", CancellationToken.None);

        Assert.Equal("AMD", updated.Name);
    }

    [Fact]
    public async Task DeleteAsync_WhenBrandHasNoProducts_ShouldRemove()
    {
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);

        await _service.DeleteAsync(brand.Id, CancellationToken.None);

        _context.ChangeTracker.Clear();
        var deleted = await _context.Brands.FindAsync(brand.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenBrandHasProducts_ShouldThrowDomainException()
    {
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);

        var product = Product.Create("Ryzen", 2899.90m, 10, brand.Id);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<DomainException>(
            () => _service.DeleteAsync(brand.Id, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnBrand()
    {
        var brand = await _service.CreateAsync("AMD", CancellationToken.None);

        var found = await _service.GetByIdAsync(brand.Id, CancellationToken.None);

        Assert.Equal(brand.Id, found.Id);
        Assert.Equal("AMD", found.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(999, CancellationToken.None));
    }

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
        await _service.CreateAsync("AMD", CancellationToken.None);
        await _service.CreateAsync("Intel", CancellationToken.None);
        await _service.CreateAsync("NVIDIA", CancellationToken.None);

        var results = await _service.SearchByNameAsync("d", CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, b => b.Name == "AMD");
        Assert.Contains(results, b => b.Name == "NVIDIA");
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        await _service.CreateAsync("AMD", CancellationToken.None);

        var results = await _service.SearchByNameAsync("Intel", CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllAsync_WhenBrandsExist_ShouldReturnAll()
    {
        await _service.CreateAsync("AMD", CancellationToken.None);
        await _service.CreateAsync("Intel", CancellationToken.None);

        var all = await _service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBrands_ShouldReturnEmptyList()
    {
        var all = await _service.GetAllAsync(CancellationToken.None);

        Assert.Empty(all);
    }
}