using StorageSystem.Domain.Data;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Domain.Services;

public class BrandService(StorageContext context)
{
    public async Task<Brand> Register(string name)
    {
        GuardClause.AgainstNullOrEmptyName(name);

        Brand brand = new() { Name = name };
        context.Brands.Add(brand);
        await context.SaveChangesAsync();
        return brand;
    }

    public async Task<Brand> Update(int id, string newName)
    {
        GuardClause.AgainstNullOrEmptyName(newName);

        var brand = await GetById(id);

        brand.Name = newName;
        await context.SaveChangesAsync();

        return brand;
    }

    public async Task<string> Delete(int id)
    {
        var brand = await GetById(id);

        context.Brands.Remove(brand);
        await context.SaveChangesAsync();

        return brand.Name;
    }

    public async Task<Brand> GetById(int id)
    {
        GuardClause.AgainstZeroOrNegativeId(id);

        var brand = await context.Brands.FindAsync(id);

        if (brand == null)
            throw new KeyNotFoundException($"The brand with ID {id} was not found.");

        return brand;
    }

    public async Task<List<Brand>> SearchByName(string name)
    {
        GuardClause.AgainstNullOrEmptyName(name);

        return await context.Brands
            .Where(b => b.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync();
    }

    public async Task<List<Brand>> GetAll()
    {
        return await context.Brands.ToListAsync();
    }
}