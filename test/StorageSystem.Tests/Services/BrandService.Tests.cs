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

        Brand result = _service.Add(name);
        Brand? brand = _context.Brands.FirstOrDefault(b => b.Name == name);

        Assert.NotNull(brand);
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
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

    
}