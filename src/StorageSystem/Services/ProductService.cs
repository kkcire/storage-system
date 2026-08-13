using StorageSystem.Data;
using StorageSystem.Entities;

namespace StorageSystem.Services;

public class ProductService(StorageContext context)
{
    public Product Register(string name, decimal price, int quantity, int brandId)
    {
        GuardClause.ValidateNullOrEmptyName(name);
        GuardClause.ValidateZeroOrNegativePrice(price);
        GuardClause.ValidateNegativeQuantity(quantity);
        GuardClause.ValidateZeroOrNegativeId(brandId);

        string trimmedName = name.Trim();
        bool productExist = context.Products.Any(p => p.Name == trimmedName);
        var brand = context.Brands.Find(brandId);

        if (brand == null)
            throw new KeyNotFoundException($"The brand with ID {brandId} was not found.");

        if (productExist)
            throw new InvalidOperationException($"A product with the name {trimmedName} already exist");

        Product product = new() { Name = trimmedName, Price = price, Quantity = quantity, BrandId = brandId };

        context.Products.Add(product);
        context.SaveChanges();
        return product;
    }

    public Product Update(int id, string? name = null, decimal? price = null, int? quantity = null)
    {
        GuardClause.ValidateZeroOrNegativeId(id);

        var product = GetById(id);

        if (name is not null)
        {
            GuardClause.ValidateNullOrEmptyName(name);

            product.Name = name.Trim();
        }

        if (price is not null)
        {
            GuardClause.ValidateZeroOrNegativePrice(price.Value);

            product.Price = price.Value;
        }

        if (quantity is not null)
        {
            GuardClause.ValidateNegativeQuantity(quantity.Value);

            product.Quantity = quantity.Value;
        }

        context.SaveChanges();
        return product;
    }

    public Product Delete(int id)
    {
        var product = GetById(id);

        context.Products.Remove(product);
        context.SaveChanges();

        return product;
    }


    public Product GetById(int id)
    {
        GuardClause.ValidateZeroOrNegativeId(id);

        var product = context.Products.Find(id);

        if (product == null)
            throw new KeyNotFoundException($"The product with ID {id} was not found.");

        return product;
    }

    public List<Product> SearchByName(string name)
    {
        GuardClause.ValidateNullOrEmptyName(name);

        return context.Products.Where(p => p.Name.Contains(name)).ToList();
    }

    public List<Product> GetAll()
    {
        return context.Products.ToList();
    }

    public Product AdjustStockQuantity(int id, int amount)
    {
        var product = GetById(id);

        product.Quantity += amount;
        context.SaveChanges();

        return product;
    }
}