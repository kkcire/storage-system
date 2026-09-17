using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Domain.Data;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Services;

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

    private async Task<Product> CreateDefaultProduct(string name = "Processador Ryzen 7 7800X3D", decimal price = 2899.90m, int quantity = 10, int brandId = 1)
    {
        return await _service.Register(name, price, quantity, brandId);
    }

    private Brand CreateDefaultBrand(string name = "AMD")
    {
        Brand brand = new() { Name = name };
        _context.Brands.Add(brand);
        _context.SaveChanges();
        return brand;
    }

    [Fact]
    public async Task Register_WithAllValidInputs_ReturnsNewProduct()
    {
        string name = "Processador Ryzen 7 7800X3D";
        decimal price = 2899.90m;
        int quantity = 10;
        int brandId = 1;

        Brand brand = CreateDefaultBrand();
        Product product = await _service.Register(name, price, quantity, brand.Id);

        Assert.NotNull(product);
        Assert.Equal(name, product.Name);
        Assert.Equal(price, product.Price);
        Assert.Equal(quantity, product.Quantity);
        Assert.Equal(brandId, product.BrandId);
    }

    [Fact]
    public async Task Register_WithProductAlreadyInDatabase_ThrowsInvalidOperationException()
    {
        string name = "Processador Ryzen 7 7800X3D";
        decimal price = 2899.90m;
        int quantity = 10;
        int brandId = 1;

        Brand brand = CreateDefaultBrand();

        Product persistedProduct = await _service.Register(name, price, quantity, brand.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(()
            => _service.Register(name, price, quantity, brandId));
    }

    [Fact]
    public async Task Register_WithNameContainingWhitespaces_TrimsName()
    {
        string whitespacedName = "  Processador Ryzen 7 ";

        Brand brand = CreateDefaultBrand();
        Product product = await _service.Register(whitespacedName, 2899.90m, 10, brand.Id);

        Assert.Equal("Processador Ryzen 7", product.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task Register_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Register(
            name: invalidName!,
            price: 2899.90m,
            quantity: 10,
            brandId: 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-203)]
    [InlineData(-0.21)]
    public async Task Register_WithInvalidPrice_ThrowsArgumentOutOfRangeException(decimal invalidPrice)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: invalidPrice,
            quantity: 10,
            brandId: 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-203)]
    public async Task Register_WithInvalidQuantity_ThrowsArgumentOutOfRangeException(int invalidQuantity)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: 2890.90m,
            quantity: invalidQuantity,
            brandId: 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Register_WithInvalidBrandId_ThrowsArgumentOutOfRangeException(int invalidBrandId)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: 2890.90m,
            quantity: 10,
            brandId: invalidBrandId));
    }

    [Fact]
    public async Task Register_WithNonExistingBrandId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 237;

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Register(
            name: "Processador Ryzen 7 7800X3D",
            price: 2890.90m,
            quantity: 10,
            brandId: nonExistingId));
    }

    [Fact]
    public async Task Update_WithAllValidInputs_UpdatesProduct()
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = CreateDefaultBrand();

        Product product = await CreateDefaultProduct();

        await _service.Update(product.Id, newName, newPrice, newQuantity);

        Assert.NotNull(product);
        Assert.Equal(newName, product.Name);
        Assert.Equal(newPrice, product.Price);
        Assert.Equal(newQuantity, product.Quantity);
    }

    [Fact]
    public async Task Update_WithNotAllValidInputs_UpdatesOnlySelectedInputs()
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = CreateDefaultBrand();

        Product product = await CreateDefaultProduct();

        await _service.Update(id: product.Id, name: newName, quantity: newQuantity);

        Assert.NotNull(product);
        Assert.Equal(newName, product.Name);
        Assert.NotEqual(newPrice, product.Price);
        Assert.Equal(newQuantity, product.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Update_WithInvalidId_ThrowsArgumentOutOfRangeException(int invalidId)
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = CreateDefaultBrand();

        Product product = await CreateDefaultProduct();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()
            => _service.Update(invalidId, newName, newPrice, newQuantity));
    }

    [Theory]
    [InlineData("")]
    public async Task Update_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = CreateDefaultBrand();

        Product product = await CreateDefaultProduct();

        await Assert.ThrowsAsync<ArgumentException>(()
            => _service.Update(product.Id, invalidName!, newPrice, newQuantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Update_WithInvalidPrice_ThrowsArgumentOutOfRangeException(decimal invalidPrice)
    {
        string newName = "Processador Ryzen 7 9700X";
        int newQuantity = 7;

        Brand brand = CreateDefaultBrand();

        Product product = await CreateDefaultProduct();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()
            => _service.Update(product.Id, newName, invalidPrice, newQuantity));
    }

    [Theory]
    [InlineData(-1)]
    public async Task Update_WithInvalidQuantity_ThrowsArgumentOutOfRangeException(int invalidQuantity)
    {
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;

        Brand brand = CreateDefaultBrand();

        Product product = await CreateDefaultProduct();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()
            => _service.Update(product.Id, newName, newPrice, invalidQuantity));
    }

    [Fact]
    public async Task Update_WithNonExistingProductId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 237;
        string newName = "Processador Ryzen 7 9700X";
        decimal newPrice = 2899.99m;
        int newQuantity = 7;

        Brand brand = CreateDefaultBrand();

        Product product = await CreateDefaultProduct();

        await Assert.ThrowsAsync<KeyNotFoundException>(()
            => _service.Update(nonExistingId, newName, newPrice, newQuantity));
    }

    [Fact]
    public async Task Update_WithExistingNameOfOtherProduct_ThrowsInvalidOperationException()
    {
        string newName = "Processador Ryzen 7 9700X";

        Brand brand = CreateDefaultBrand();

        Product existingProduct = await CreateDefaultProduct("Processador Ryzen 7 9700X", 2899.99m, 7, brand.Id);

        Product product = await CreateDefaultProduct();

        await Assert.ThrowsAsync<InvalidOperationException>(()
            => _service.Update(id: product.Id, name: newName));
    }

    [Fact]
    public async Task Delete_WithValidId_RemovesProductFromDatabase()
    {
        Brand brand = CreateDefaultBrand();
        Product product = await CreateDefaultProduct();

        await _service.Delete(product.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(()
            => _service.GetById(product.Id));
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsDeletedProductInformations()
    {
        Brand brand = CreateDefaultBrand();
        Product product = await CreateDefaultProduct();

        Product deletedProduct = await _service.Delete(product.Id);

        Assert.Equal(product.Id, deletedProduct.Id);
        Assert.Equal(product.Name, deletedProduct.Name);
        Assert.Equal(product.Price, deletedProduct.Price);
        Assert.Equal(product.Quantity, deletedProduct.Quantity);
        Assert.Equal(product.BrandId, deletedProduct.BrandId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Delete_WithInvalidId_ThrowsArgumentOutOfRangeException(int invalidId)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.Delete(invalidId));
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 237;

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.Delete(nonExistingId));
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsProduct()
    {
        Brand brand = CreateDefaultBrand();
        Product product = await CreateDefaultProduct();
        Product foundProduct = await _service.GetById(product.Id);

        Assert.NotNull(foundProduct);
        Assert.Equal(product, foundProduct);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task GetById_WithInvalidId_ThrowsArgumentOutOfrangeArgumentException(int id)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.GetById(id));
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        int nonExistingId = 256;

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetById(nonExistingId));
    }

    [Fact]
    public async Task SearchByName_WithValidSearch_ReturnsMatchingProducts()
    {
        Brand amdBrand = CreateDefaultBrand("AMD");
        Brand intelBrand = CreateDefaultBrand("Intel");

        Product ryzenProduct9700 = await CreateDefaultProduct(name: "Processador Ryzen 7 9700X", brandId: amdBrand.Id);
        Product ryzenProduct5600 = await CreateDefaultProduct(name: "Processador Ryzen 5 5600", brandId: amdBrand.Id);
        Product intelProduct = await CreateDefaultProduct(name: "Processador Intel Core i7-14700K", brandId: intelBrand.Id);

        List<Product> foundMatchingProducts = await _service.SearchByName("zen");

        Assert.NotNull(foundMatchingProducts);
        Assert.Contains(foundMatchingProducts, p => p.Name == "Processador Ryzen 7 9700X");
        Assert.Contains(foundMatchingProducts, p => p.Name == "Processador Ryzen 5 5600");
        Assert.All(foundMatchingProducts, p => Assert.Contains("zen", p.Name));
    }

    [Fact]
    public async Task SearchByName_IsCaseInsensitive_ReturnsMatchingProducts()
    {
        Brand amdBrand = CreateDefaultBrand("AMD");
        Product ryzenProduct9700 = await CreateDefaultProduct(name: "Processador Ryzen 7 9700X", brandId: amdBrand.Id);

        List<Product> foundMatchingProducts = await _service.SearchByName("ryzen");

        Assert.Contains(foundMatchingProducts, p => p.Name == "Processador Ryzen 7 9700X");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SearchByName_WithNullOrEmptyName_ThrowsArgumentException(string? searchName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchByName(searchName!));
    }

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        Brand amdBrand = CreateDefaultBrand("AMD");
        Brand intelBrand = CreateDefaultBrand("Intel");

        Product ryzenProduct9700 = await CreateDefaultProduct(name: "Processador Ryzen 7 9700X", brandId: amdBrand.Id);
        Product ryzenProduct5600 = await CreateDefaultProduct(name: "Processador Ryzen 5 5600", brandId: amdBrand.Id);
        Product intelProduct = await CreateDefaultProduct(name: "Processador Intel Core i7-14700K", brandId: intelBrand.Id);

        List<Product> products = await _service.GetAll();

        Assert.NotNull(products);
        Assert.Equal(3, products.Count);
        Assert.All(products, p => Assert.NotEqual(0, p.Id));
    }

    [Fact]
    public async Task GetAll_WithNoProducts_ReturnsEmptyList()
    {
        List<Product> products = await _service.GetAll();

        Assert.Empty(products);
    }
}