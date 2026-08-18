using Microsoft.Data.Sqlite;
using StorageSystem.Data;
using StorageSystem.Entities;
using StorageSystem.Services;

namespace StorageSystem.Tests.Services;

public class ProductServiceTests
{
    private readonly StorageContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        var connection = new SqliteConnection("Data Source = :memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;

        _context = new(options);
        _context.Database.EnsureCreated();

        _service = new(_context);

    }

    [Fact]
    public void Register_WithAllValidInputs_ReturnsNewProduct()
    {
        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        string name = "Processador Ryzen 7 7800X3D";
        decimal price = 2899.90m;
        int quantity = 10;
        int brandId = 1;

        Product product = _service.Register(name, price, quantity, brand.Id);

        Assert.NotNull(product);
        Assert.Equal(name, product.Name);
        Assert.Equal(price, product.Price);
        Assert.Equal(quantity, product.Quantity);
        Assert.Equal(brandId, product.BrandId);
    }

    [Fact]
    public void Register_WithProductAlreadyInDatabase_ThrowsInvalidOperationException()
    {
        string name = "Processador Ryzen 7 7800X3D";
        decimal price = 2899.90m;
        int quantity = 10;
        int brandId = 1;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product persistedProduct = _service.Register(name, price, quantity, brand.Id);

        Assert.Throws<InvalidOperationException>(()
            => _service.Register(name, price, quantity, brandId));
    }

    [Fact]
    public void Register_WithNameContainingWhitespaces_TrimsName()
    {
        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        string whitespacedName = "  Processador Ryzen 7 ";

        Product product = _service.Register(whitespacedName, 2899.90m, 10, brand.Id);

        Assert.Equal("Processador Ryzen 7", product.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public void Register_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => _service.Register(
            name: invalidName,
            price: 2899.90m,
            quantity: 10,
            brandId: 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-203)]
    [InlineData(-0.21)]
    public void Register_WithInvalidPrice_ThrowsArgumentOutOfRangeException(decimal invalidPrice)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: invalidPrice,
            quantity: 10,
            brandId: 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-203)]
    public void Register_WithInvalidQuantity_ThrowsArgumentOutOfRangeException(int invalidQuantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: 2890.90m,
            quantity: invalidQuantity,
            brandId: 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Register_WithInvalidBrandId_ThrowsArgumentOutOfRangeException(int invalidBrandId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: 2890.90m,
            quantity: 10,
            brandId: invalidBrandId));
    }

    [Fact]
    public void Register_WithNonExistingBrandId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 237;

        Assert.Throws<KeyNotFoundException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: 2890.90m,
            quantity: 10,
            brandId: nonExistingId));
    }

    [Fact]
    public void Update_WithAllValidInputs_UpdatesProduct()
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product product = _service.Register("Processador Ryzen 7 7800X3D", 2899.90m, 10, brand.Id);

        _service.Update(product.Id, newName, newPrice, newQuantity);

        Assert.NotNull(product);
        Assert.Equal(newName, product.Name);
        Assert.Equal(newPrice, product.Price);
        Assert.Equal(newQuantity, product.Quantity);
    }

    [Fact]
    public void Update_WithNotAllValidInputs_UpdatesOnlySelectedInputs()
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product product = _service.Register("Processador Ryzen 7 7800X3D", 2899.90m, 10, brand.Id);

        _service.Update(id: product.Id, name: newName, quantity: newQuantity);

        Assert.NotNull(product);
        Assert.Equal(newName, product.Name);
        Assert.NotEqual(newPrice, product.Price);
        Assert.Equal(newQuantity, product.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithInvalidName_ThrowsArgumentOutOfRangeException(int invalidId)
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product product = _service.Register("Processador Ryzen 7 7800X3D", 2899.90m, 10, brand.Id);

        Assert.Throws<ArgumentOutOfRangeException>(()
            => _service.Update(invalidId, newName, newPrice, newQuantity));
    }

    [Theory]
    [InlineData("")]
    public void Update_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product product = _service.Register("Processador Ryzen 7 7800X3D", 2899.90m, 10, brand.Id);

        Assert.Throws<ArgumentException>(()
            => _service.Update(product.Id, invalidName, newPrice, newQuantity));

    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithInvalidPrice_ThrowsArgumentOutOfRangeException(decimal invalidPrice)
    {
        string newName = "Processador Ryzen 7 9700X";
        int newQuantity = 7;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product product = _service.Register("Processador Ryzen 7 7800X3D", 2899.90m, 10, brand.Id);

        Assert.Throws<ArgumentOutOfRangeException>(()
            => _service.Update(product.Id, newName, invalidPrice, newQuantity));

    }

    [Theory]
    [InlineData(-1)]
    public void Update_WithInvalidQuantity_ThrowsArgumentOutOfRangeException(int invalidQuantity)
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product product = _service.Register("Processador Ryzen 7 7800X3D", 2899.90m, 10, brand.Id);

        Assert.Throws<ArgumentOutOfRangeException>(()
            => _service.Update(product.Id, newName, newPrice, invalidQuantity));

    }

    [Fact]
    public void Update_WithNonExistingProductId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 237;
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = new() { Name = "AMD" };
        _context.Brands.Add(brand);
        _context.SaveChanges();

        Product product = _service.Register("Processador Ryzen 7 7800X3D", 2899.90m, 10, brand.Id);

        Assert.Throws<KeyNotFoundException>(() 
            => _service.Update(nonExistingId, newName, newPrice, newQuantity));

    }
}