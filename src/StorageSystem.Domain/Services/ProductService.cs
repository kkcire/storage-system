using StorageSystem.Domain.Data;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Services;

public class ProductService(StorageContext context)
{
    public async Task<Product> Register(string name, decimal price, int quantity, int brandId)
    {
        GuardClause.AgainstNullOrEmptyName(name);
        GuardClause.AgainstZeroOrNegativePrice(price);
        GuardClause.AgainstNegativeQuantity(quantity);
        GuardClause.AgainstZeroOrNegativeId(brandId);

        string trimmedName = name.Trim();
        bool productExist = await context.Products.AnyAsync(p => p.Name == trimmedName);
        bool brandExist = await context.Brands.AnyAsync(b => b.Id == brandId);
    
        if (!brandExist)
            throw new KeyNotFoundException($"The brand with ID {brandId} was not found.");

        if (productExist)
            throw new InvalidOperationException($"A product with the name {trimmedName} already exist");

        Product product = new() { Name = trimmedName, Price = price, Quantity = quantity, BrandId = brandId };

        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> Update(int id, string? name = null, decimal? price = null, int? quantity = null)
    {
        var product = await GetById(id);

        if (name is not null)
        {
            GuardClause.AgainstNullOrEmptyName(name);

            string trimmedName = name.Trim();

            bool identicalProductExist = await context.Products.AnyAsync(p => p.Name == trimmedName && p.Id != id);

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

        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> Delete(int id)
    {
        var product = await GetById(id);

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return product;
    }


    public async Task<Product> GetById(int id)
    {
        GuardClause.AgainstZeroOrNegativeId(id);

        var product = await context.Products.FindAsync(id);

        if (product == null)
            throw new KeyNotFoundException($"The product with ID {id} was not found.");

        return product;
    }

    public async Task<List<Product>> SearchByName(string name)
    {
        GuardClause.AgainstNullOrEmptyName(name);

        return await context.Products
<<<<<<< HEAD
            .Where(p => p.Name.ToLower().Contains(name.ToLower()))
=======
            .Where(p => p.Name.ToLower().Contains(name))
>>>>>>> 85c5814c8a03804b02e2c38c0715dd7164f7ce4d
            .ToListAsync();
    }

    public async Task<List<Product>> GetAll()
    {
        return await context.Products.ToListAsync();
    }

    public async Task<Product> AdjustStockQuantity(int id, int amount)
    {
        var product = await GetById(id);

        if (amount + product.Quantity < 0)
            throw new InvalidOperationException("The value cannot leaves the stock negative");

        product.Quantity += amount;
        await context.SaveChangesAsync();

        return product;
    }
}