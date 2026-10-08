using StorageSystem.Domain.Exceptions;
using StorageSystem.Domain.Guards;

namespace StorageSystem.Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public int BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    private Product() { } // EF Core

    public static Product Create(string name, decimal price, int quantity, int brandId)
    {
        GuardClause.AgainstNullOrWhiteSpaceName(name);
        GuardClause.AgainstNegativePrice(price);
        GuardClause.AgainstNegativeQuantity(quantity);
        GuardClause.AgainstZeroOrNegativeId(brandId);

        return new Product
        {
            Name = name.Trim(),
            Price = price,
            Quantity = quantity,
            BrandId = brandId
        };
    }

    public void Rename(string newName)
    {
        GuardClause.AgainstNullOrWhiteSpaceName(newName);

        Name = newName.Trim();
    }

    public void ChangePrice(decimal newPrice)
    {
        GuardClause.AgainstNegativePrice(newPrice);

        Price = newPrice;
    }

    public void ChangeQuantity(int newQuantity)
    {
        GuardClause.AgainstNegativeQuantity(newQuantity);

        Quantity = newQuantity;
    }

    public void ChangeBrand(int newBrandId)
    {
        GuardClause.AgainstZeroOrNegativeId(newBrandId);

        if (newBrandId == BrandId)
            return; // No change needed

        BrandId = newBrandId;
    }

    public void Update(string newName, decimal newPrice, int newQuantity)
    {
        Rename(newName);
        ChangePrice(newPrice);
        ChangeQuantity(newQuantity);
    }

    public void AddStock(int amount)
    {
        if (Quantity + amount < 0)
        {
            throw new DomainException("Stock adjustment would result in negative quantity.");
        }

        Quantity += amount;
    }
}