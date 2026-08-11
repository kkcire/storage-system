using Microsoft.Data.Sqlite;
using StorageSystem.Data;
using StorageSystem.Entities;
using StorageSystem.Services;

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
    public void Add_WithValidName_ReturnsNewBrand()
    {
        string name = "Spotify";

        Brand brand = _service.Add(name);

        Assert.NotNull(brand);
        Assert.Equal(name, brand.Name);
        Assert.NotEqual(0, brand.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => _service.Add(invalidName));
    }

    [Fact]
    public void Update_WithValidIdAndName_UpdatesBrand()
    {
        Brand brand = _service.Add("Old Name");

        Brand result = _service.Update(brand.Id, "New Name");

        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public void Update_WithNonExistingId_ThrowsKeyNotFoundExcepton()
    {
        Assert.Throws<KeyNotFoundException>(() => _service.Update(999, "Samsung"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => _service.Update(1, invalidName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Update_WithInvalidId_ThrowsOutOfRangeArgumentException(int invalidId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Update(invalidId, "Spotify"));
    }

    [Fact]
    public void GetById_WithValidId_ReturnsBrand()
    {

        Brand brand = _service.Add("Fender");
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void SearchByName_WithNullOrEmptyName_ThrowsArgumentException(string searchName)
    {
        Assert.Throws<ArgumentException>(() => _service.SearchByName(searchName));
    }

}