namespace StorageSystem.Tests.Domain;

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

    [Fact]
    public void Create_WhenAllInputsAreValid_ShouldReturnProduct()
    {
        var product = CreateDefaultProduct();

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
        Assert.Throws<DomainException>(() => CreateDefaultProduct(name: name!));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WhenPriceIsNegative_ShouldThrowDomainException(decimal price)
    {
        Assert.Throws<DomainException>(() => CreateDefaultProduct(price: price));
    }

    [Fact]
    public void Create_WhenPriceIsZero_ShouldReturnProduct()
    {
        var product = CreateDefaultProduct(price: 0m);
        Assert.Equal(0m, product.Price);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WhenQuantityIsNegative_ShouldThrowDomainException(int quantity)
    {
        Assert.Throws<DomainException>(() => CreateDefaultProduct(quantity: quantity));
    }

    [Fact]
    public void Create_WhenQuantityIsZero_ShouldReturnProduct()
    {
        var product = CreateDefaultProduct(quantity: 0);
        Assert.Equal(0, product.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenBrandIdIsZeroOrNegative_ShouldThrowDomainException(int brandId)
    {
        Assert.Throws<DomainException>(() => CreateDefaultProduct(brandId: brandId));
    }

    [Theory]
    [InlineData("  Product  ", "Product")]
    [InlineData("  Another  ", "Another")]
    public void Create_WhenNameIsNotTrimmed_ShouldTrimName(string name, string expected)
    {
        var product = CreateDefaultProduct(name: name);
        Assert.Equal(expected, product.Name);
    }

    [Fact]
    public void Rename_WhenNameIsValid_ShouldUpdateName()
    {
        var product = CreateDefaultProduct();

        product.Rename("New Name");

        Assert.Equal("New Name", product.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WhenNameIsNullOrWhiteSpace_ShouldThrowDomainException(string? newName)
    {
        var product = CreateDefaultProduct();

        Assert.Throws<DomainException>(() => product.Rename(newName!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WhenNameIsInvalid_ShouldNotChangeName(string? newName)
    {
        var product = CreateDefaultProduct();

        Assert.Throws<DomainException>(() => product.Rename(newName!));
        Assert.Equal(DefaultName, product.Name);
    }

    [Theory]
    [InlineData("  New Name  ", "New Name")]
    [InlineData("  Another  ", "Another")]
    public void Rename_WhenNameIsNotTrimmed_ShouldTrimName(string newName, string expected)
    {
        var product = CreateDefaultProduct();

        product.Rename(newName);

        Assert.Equal(expected, product.Name);
    }

    [Fact]
    public void ChangePrice_WhenPriceIsValid_ShouldUpdatePrice()
    {
        var product = CreateDefaultProduct();

        product.ChangePrice(20.50m);

        Assert.Equal(20.50m, product.Price);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void ChangePrice_WhenPriceIsNegative_ShouldThrowDomainException(decimal newPrice)
    {
        var product = CreateDefaultProduct();

        Assert.Throws<DomainException>(() => product.ChangePrice(newPrice));
    }

    [Fact]
    public void ChangePrice_WhenPriceIsZero_ShouldUpdatePrice()
    {
        var product = CreateDefaultProduct();

        product.ChangePrice(0m);

        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void ChangePrice_WhenPriceIsNegative_ShouldNotChangePrice()
    {
        var product = CreateDefaultProduct();

        Assert.Throws<DomainException>(() => product.ChangePrice(-1m));
        Assert.Equal(DefaultPrice, product.Price);
    }

    [Fact]
    public void ChangeQuantity_WhenQuantityIsValid_ShouldUpdateQuantity()
    {
        var product = CreateDefaultProduct();

        product.ChangeQuantity(20);

        Assert.Equal(20, product.Quantity);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ChangeQuantity_WhenQuantityIsNegative_ShouldThrowDomainException(int newQuantity)
    {
        var product = CreateDefaultProduct();

        Assert.Throws<DomainException>(() => product.ChangeQuantity(newQuantity));
    }

    [Fact]
    public void ChangeQuantity_WhenQuantityIsZero_ShouldUpdateQuantity()
    {
        var product = CreateDefaultProduct();

        product.ChangeQuantity(0);

        Assert.Equal(0, product.Quantity);
    }

    [Fact]
    public void Update_WhenAllInputsAreValid_ShouldUpdateAllFields()
    {
        var product = CreateDefaultProduct();

        product.Update("New Name", 20.50m, 20);

        Assert.Multiple(
            () => Assert.Equal("New Name", product.Name),
            () => Assert.Equal(20.50m, product.Price),
            () => Assert.Equal(20, product.Quantity));
    }

    [Fact]
    public void AddStock_WhenAmountIsPositive_ShouldIncreaseQuantity()
    {
        var product = CreateDefaultProduct();

        product.AddStock(5);

        Assert.Equal(15, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenAmountIsNegative_ShouldDecreaseQuantity()
    {
        var product = CreateDefaultProduct();

        product.AddStock(-3);

        Assert.Equal(7, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenWouldGoNegative_ShouldThrowDomainException()
    {
        var product = CreateDefaultProduct();

        Assert.Throws<DomainException>(() => product.AddStock(-20));
    }

    [Fact]
    public void AddStock_WhenWouldGoNegative_ShouldNotChangeQuantity()
    {
        var product = CreateDefaultProduct();

        Assert.Throws<DomainException>(() => product.AddStock(-20));
        Assert.Equal(DefaultQuantity, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenAmountIsZero_ShouldKeepQuantity()
    {
        var product = CreateDefaultProduct();

        product.AddStock(0);

        Assert.Equal(DefaultQuantity, product.Quantity);
    }

    [Fact]
    public void AddStock_WhenAmountResultsInZero_ShouldSetQuantityToZero()
    {
        var product = CreateDefaultProduct();

        product.AddStock(-10);

        Assert.Equal(0, product.Quantity);
    }

    [Fact]
    public void ChangeBrand_WhenBrandIdIsValid_ShouldUpdateBrandId()
    {
        var product = CreateDefaultProduct(brandId: 1);

        product.ChangeBrand(2);

        Assert.Equal(2, product.BrandId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeBrand_WhenBrandIdIsZeroOrNegative_ShouldThrowDomainException(int newBrandId)
    {
        var product = CreateDefaultProduct(brandId: 1);

        Assert.Throws<DomainException>(() => product.ChangeBrand(newBrandId));
    }

    [Fact]
    public void ChangeBrand_WhenSameBrandId_ShouldKeepBrandId()
    {
        var product = CreateDefaultProduct(brandId: 1);

        product.ChangeBrand(1);

        Assert.Equal(1, product.BrandId);
    }
}