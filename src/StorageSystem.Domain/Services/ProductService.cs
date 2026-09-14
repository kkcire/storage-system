using StorageSystem.Domain.Data;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Services;

public class ProductService(StorageContext context)
{
    public Product Register(string name, decimal price, int quantity, int brandId)
    {
        GuardClause.AgainstNullOrEmptyName(name);
        GuardClause.AgainstZeroOrNegativePrice(price);
        GuardClause.AgainstNegativeQuantity(quantity);
        GuardClause.AgainstZeroOrNegativeId(brandId);

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
        var product = GetById(id);

        if (name is not null)
        {
            GuardClause.AgainstNullOrEmptyName(name);

            string trimmedName = name.Trim();

            bool identicalProductExist = context.Products.Any(p => p.Name == trimmedName && p.Id != id);

            if (identicalProductExist)
                throw new InvalidOperationException($"A product with the name {trimmedName} already exists");

            product.Name = trimmedName;
        }


        if (price is not null)
        {
            GuardClause.AgainstZeroOrNegativePrice(price.Value);

            product.Price = price.Value;
        }

        if (quantity is not null)
        {
            GuardClause.AgainstNegativeQuantity(quantity.Value);

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
        GuardClause.AgainstZeroOrNegativeId(id);

        var product = context.Products.Find(id);

        if (product == null)
            throw new KeyNotFoundException($"The product with ID {id} was not found.");

        return product;
    }

    public List<Product> SearchByName(string name)
    {
        GuardClause.AgainstNullOrEmptyName(name);

        return context.Products
            .Where(p => p.Name.ToLower().Contains(name.ToLower()))
            .ToList();
    }

    public List<Product> GetAll()
    {
        return context.Products.ToList();
    }

    public Product AdjustStockQuantity(int id, int amount)
    {
        var product = GetById(id);

        if (amount + product.Quantity < 0)
            throw new InvalidOperationException("The value cannot leaves the stock negative");

        product.Quantity += amount;
        context.SaveChanges();

        return product;
    }
}