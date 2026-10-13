namespace StorageSystem.Tests.Domain;

[Trait("Category", "Unit")]
public class ProductTests
{
    private const string DefaultName = "Product";
    private const decimal DefaultPrice = 10.99m;
    private const int DefaultQuantity = 10;
    private const int DefaultBrandId = 1;

    private static Product CreateDefaultProduct(
        string name = DefaultName,
        decimal price = DefaultPrice,
        int quantity = DefaultQuantity,
        int brandId = DefaultBrandId)
        => Product.Create(name, price, quantity, brandId);

    // ---------- Create ----------

    [Fact]
    public void Create_WhenAllInputsAreValid_ShouldReturnProduct()
    {
        // Act
        var product = CreateDefaultProduct();

        // Assert
        Assert.Multiple(
            () => Assert.Equal(DefaultName, product.Name),
            () => Assert.Equal(DefaultPrice, product.Price),
            () => Assert.Equal(DefaultQuantity, product.Quantity),
            () => Assert.Equal(DefaultBrandId, product.BrandId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsNullOrWhiteSpace_ShouldThrowDomainException(string? name)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => CreateDefaultProduct(name: name!));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WhenPriceIsNegative_ShouldThrowDomainException(decimal price)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => CreateDefaultProduct(price: price));
    }

    [Fact]
    public void Create_WhenPriceIsZero_ShouldReturnProduct()
    {
        // Act
        var product = CreateDefaultProduct(price: 0m);

        // Assert
        Assert.Equal(0m, product.Price);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WhenQuantityIsNegative_ShouldThrowDomainException(int quantity)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => CreateDefaultProduct(quantity: quantity));
    }

    [Fact]
    public void Create_WhenQuantityIsZero_ShouldReturnProduct()
    {
        // Act
        var product = CreateDefaultProduct(quantity: 0);

        // Assert
        Assert.Equal(0, product.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenBrandIdIsZeroOrNegative_ShouldThrowDomainException(int brandId)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => CreateDefaultProduct(brandId: brandId));
    }

    [Theory]
    [InlineData("  Product  ", "Product")]
    [InlineData("  Another  ", "Another")]
    public void Create_WhenNameIsNotTrimmed_ShouldTrimName(string name, string expected)
    {
        // Act
        var product = CreateDefaultProduct(name: name);

        // Assert
        Assert.Equal(expected, product.Name);
    }

    // ---------- Rename ----------

    [Fact]
    public void Rename_WhenNameIsValid_ShouldUpdateName()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.Rename("New Name");

        // Assert
        Assert.Equal("New Name", product.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WhenNameIsNullOrWhiteSpace_ShouldThrowDomainException(string? newName)
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.Rename(newName!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WhenNameIsInvalid_ShouldNotChangeName(string? newName)
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        Assert.Throws<DomainException>(() => product.Rename(newName!));

        // Assert
        Assert.Equal(DefaultName, product.Name);
    }

    [Theory]
    [InlineData("  New Name  ", "New Name")]
    [InlineData("  Another  ", "Another")]
    public void Rename_WhenNameIsNotTrimmed_ShouldTrimName(string newName, string expected)
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.Rename(newName);

        // Assert
        Assert.Equal(expected, product.Name);
    }

    // ---------- ChangePrice ----------

    [Fact]
    public void ChangePrice_WhenPriceIsValid_ShouldUpdatePrice()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.ChangePrice(20.50m);

        // Assert
        Assert.Equal(20.50m, product.Price);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void ChangePrice_WhenPriceIsNegative_ShouldThrowDomainException(decimal newPrice)
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.ChangePrice(newPrice));
    }

    [Fact]
    public void ChangePrice_WhenPriceIsZero_ShouldUpdatePrice()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.ChangePrice(0m);

        // Assert
        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void ChangePrice_WhenPriceIsNegative_ShouldNotChangePrice()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        Assert.Throws<DomainException>(() => product.ChangePrice(-1m));

        // Assert
        Assert.Equal(DefaultPrice, product.Price);
    }

    // ---------- ChangeQuantity ----------

    [Fact]
    public void ChangeQuantity_WhenQuantityIsValid_ShouldUpdateQuantity()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.ChangeQuantity(20);

        // Assert
        Assert.Equal(20, product.Quantity);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ChangeQuantity_WhenQuantityIsNegative_ShouldThrowDomainException(int newQuantity)
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.ChangeQuantity(newQuantity));
    }

    [Fact]
    public void ChangeQuantity_WhenQuantityIsZero_ShouldUpdateQuantity()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.ChangeQuantity(0);

        // Assert
        Assert.Equal(0, product.Quantity);
    }

    // ---------- Update ----------

    [Fact]
    public void Update_WhenAllInputsAreValid_ShouldUpdateAllFields()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.Update("New Name", 20.50m, 20);

        // Assert
        Assert.Multiple(
            () => Assert.Equal("New Name", product.Name),
            () => Assert.Equal(20.50m, product.Price),
            () => Assert.Equal(20, product.Quantity));
    }

    // ---------- AddStock ----------

    [Fact]
    public void AddStock_WhenAmountIsPositive_ShouldIncreaseQuantity()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.AddStock(5);

        // Assert
        Assert.Equal(15, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenAmountIsNegative_ShouldDecreaseQuantity()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.AddStock(-3);

        // Assert
        Assert.Equal(7, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenWouldGoNegative_ShouldThrowDomainException()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.AddStock(-20));
    }

    [Fact]
    public void AddStock_WhenWouldGoNegative_ShouldNotChangeQuantity()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        Assert.Throws<DomainException>(() => product.AddStock(-20));

        // Assert
        Assert.Equal(DefaultQuantity, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenAmountIsZero_ShouldKeepQuantity()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.AddStock(0);

        // Assert
        Assert.Equal(DefaultQuantity, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenAmountResultsInZero_ShouldSetQuantityToZero()
    {
        // Arrange
        var product = CreateDefaultProduct();

        // Act
        product.AddStock(-10);

        // Assert
        Assert.Equal(0, product.Quantity);
    }

    // ---------- ChangeBrand ----------

    [Fact]
    public void ChangeBrand_WhenBrandIdIsValid_ShouldUpdateBrandId()
    {
        // Arrange
        var product = CreateDefaultProduct(brandId: 1);

        // Act
        product.ChangeBrand(2);

        // Assert
        Assert.Equal(2, product.BrandId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeBrand_WhenBrandIdIsZeroOrNegative_ShouldThrowDomainException(int newBrandId)
    {
        // Arrange
        var product = CreateDefaultProduct(brandId: 1);

        // Act & Assert
        Assert.Throws<DomainException>(() => product.ChangeBrand(newBrandId));
    }

    [Fact]
    public void ChangeBrand_WhenSameBrandId_ShouldKeepBrandId()
    {
        // Arrange
        var product = CreateDefaultProduct(brandId: 1);

        // Act
        product.ChangeBrand(1);

        // Assert
        Assert.Equal(1, product.BrandId);
    }
}