namespace StorageSystem.Tests.Domain;

public class BrandTests
{
    // ---------- Create ----------

    [Fact]
    public void Create_WhenNameIsValid_ShouldReturnBrand()
    {
        // Arrange
        string name = "Brand";

        // Act
        var brand = Brand.Create(name);

        // Assert
        Assert.Equal(name, brand.Name);
    }

    [Fact]
    public void Create_WhenNewlyCreated_ShouldReturnBrandWithEmptyProductsCollection()
    {
        // Arrange
        string name = "Brand";

        // Act
        var brand = Brand.Create(name);

        // Assert
        Assert.NotNull(brand.Products);
        Assert.Empty(brand.Products);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsNullOrWhiteSpace_ShouldThrowDomainException(string? name)
    {
        Assert.Throws<DomainException>(() => Brand.Create(name!));
    }

    [Theory]
    [InlineData("  New Name  ", "New Name")]
    [InlineData("  Another Brand  ", "Another Brand")]
    [InlineData("\tFamous Brand\t", "Famous Brand")]
    public void Create_WhenNameIsNotTrimmed_ShouldTrimName(string name, string expected)
    {
        // Act
        var brand = Brand.Create(name);

        // Assert
        Assert.Equal(expected, brand.Name);
    }

    // ---------- Rename ----------

    [Fact]
    public void Rename_WhenNameIsValid_ShouldUpdateBrandName()
    {
        // Arrange
        var brand = Brand.Create("Old Name");
        string newName = "New Name";

        // Act
        brand.Rename(newName);

        // Assert
        Assert.Equal(newName, brand.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WhenNameIsNullOrWhiteSpace_ShouldThrowDomainException(string? newName)
    {
        // Arrange
        var brand = Brand.Create("Old Name");

        // Act & Assert
        Assert.Throws<DomainException>(() => brand.Rename(newName!));
    }

    [Theory]
    [InlineData("  New Name  ", "New Name")]
    [InlineData("  Another Brand  ", "Another Brand")]
    [InlineData("\tFamous Brand\t", "Famous Brand")]
    public void Rename_WhenNameIsNotTrimmed_ShouldTrimName(string newName, string expected)
    {
        // Arrange
        var brand = Brand.Create("Old Name");

        // Act
        brand.Rename(newName);

        // Assert
        Assert.Equal(expected, brand.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WhenNameIsInvalid_ShouldKeepPreviousName(string? newName)
    {
        // Arrange
        var brand = Brand.Create("Old Name");
        string previousName = brand.Name;

        // Act & Assert
        Assert.Throws<DomainException>(() => brand.Rename(newName!));

        // Assert
        Assert.Equal(previousName, brand.Name);
    }
}