using Microsoft.Data.Sqlite;
using StorageSystem.Domain.Data;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Services;

namespace StorageSystem.Tests.Services;

public class BrandServiceTests
{
    private readonly StorageContext _context;
    private readonly BrandService _service;

    public BrandServiceTests()
    {
        var connection = new SqliteConnection("Data Source = :memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;

        _context = new(options);
        _context.Database.EnsureCreated();

        _service = new BrandService(_context);
    }

    [Fact]
    public void Register_WithValidName_ReturnsNewBrand()
    {
        string name = "Spotify";

        Brand brand = _service.Register(name);

        Assert.NotNull(brand);
        Assert.Equal(name, brand.Name);
        Assert.NotEqual(0, brand.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Register_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        Assert.Throws<ArgumentException>(() => _service.Register(invalidName!));
    }

    [Fact]
    public void Update_WithValidIdAndName_UpdatesBrand()
    {
        Brand brand = _service.Register("Old Name");

        Brand result = _service.Update(brand.Id, "New Name");

        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public void Update_WithNonExistingId_ThrowsKeyNotFoundExcepton()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.Update(999, "Samsung"));
    }

    [Fact]
    public void Update_WithInvalidIdAndName_ThrowsArgumentExceptionForNameFirst()
    {
        Assert.Throws<ArgumentException>(() => _service.Update(-2, ""));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        Assert.Throws<ArgumentException>(() => _service.Update(1, invalidName!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Update_WithInvalidId_ThrowsOutOfRangeArgumentException(int invalidId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Update(invalidId, "Spotify"));
    }

    [Fact]
    public void Delete_WithValidId_RemovesBrandFromDatabase()
    {
        Brand brand = new() { Name = "Fender" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        _service.Delete(brand.Id);

        Assert.Throws<KeyNotFoundException>(() => _service.GetById(brand.Id));
    }

    [Fact]
    public void Delete_WithValidId_ReturnsDeletedBrandName()
    {
        Brand brand = new() { Name = "Fender" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        string deletedName = _service.Delete(brand.Id);

        Assert.Equal("Fender", deletedName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Delete_WithInvalidId_ThrowsArgumentOutOfRangeException(int invalidId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Delete(invalidId));
    }

    [Fact]
    public void Delete_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 237;

        Assert.Throws<KeyNotFoundException>(() => _service.Delete(nonExistingId));
    }

    [Fact]
    public void GetById_WithValidId_ReturnsBrand()
    {
        Brand brand = _service.Register("Fender");
        Brand result = _service.GetById(brand.Id);

        Assert.NotNull(result);
        Assert.Equal(brand.Id, result.Id);
        Assert.Equal(brand.Name, result.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void GetById_WithInvalidId_ThrowsArgumentOutOfrangeArgumentException(int id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetById(id));
    }

    [Fact]
    public void GetById_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 256;

        Assert.Throws<KeyNotFoundException>(() => _service.GetById(nonExistingId));
    }

    [Fact]
    public void SearchByName_WithValidSearch_ReturnsMatchingBrands()
    {
        _context.Brands.Add(new Brand { Name = "Fender" });
        _context.Brands.Add(new Brand { Name = "Gibson" });
        _context.Brands.Add(new Brand { Name = "Jackson" });
        _context.SaveChanges();

        List<Brand> result = _service.SearchByName("on");

        Assert.NotNull(result);
        Assert.Contains(result, b => b.Name == "Jackson");
        Assert.Contains(result, b => b.Name == "Gibson");
        Assert.All(result, b => Assert.Contains("on", b.Name));
    }

    [Fact]
    public void SearchByName_IsCaseInsensitive_ReturnsMatchingProducts()
    {
        _context.Brands.Add(new Brand { Name = "AMD" });
        _context.SaveChanges();

        List<Brand> foundMatchingProducts = _service.SearchByName("AM");

        Assert.Contains(foundMatchingProducts, p => p.Name == "AMD");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void SearchByName_WithNullOrEmptyName_ThrowsArgumentException(string? searchName)
    {
        Assert.Throws<ArgumentException>(() => _service.SearchByName(searchName!));
    }

    [Fact]
    public void GetAll_ReturnsAllBrands()
    {
        _context.Brands.Add(new Brand { Name = "Fender" });
        _context.Brands.Add(new Brand { Name = "Gibson" });
        _context.Brands.Add(new Brand { Name = "Jackson" });
        _context.SaveChanges();

        List<Brand> result = _service.GetAll();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, b => Assert.NotEqual(0, b.Id));
    }

    [Fact]
    public void GetAll_WithNoBrands_ReturnsEmptyList()
    {
        List<Brand> result = _service.GetAll();

        Assert.Empty(result);
    }
}