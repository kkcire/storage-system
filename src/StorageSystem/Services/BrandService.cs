using StorageSystem.Data;
using StorageSystem.Entities;

namespace StorageSystem.Services;

public class BrandService(StorageContext context)
{
    public Brand Register(string name)
    {
        GuardClause.ValidateNullOrEmptyName(name);

        Brand brand = new() { Name = name };
        context.Brands.Add(brand);
        context.SaveChanges();
        return brand;
    }

    public Brand Update(int id, string newName)
    {
        GuardClause.ValidateNullOrEmptyName(newName);

        var brand = GetById(id);

        brand.Name = newName;
        context.SaveChanges();

        return brand;
    }

    public string Delete(int id)
    {
        var brand = GetById(id);

        context.Brands.Remove(brand);
        context.SaveChanges();

        return brand.Name;
    }

    public Brand GetById(int id)
    {
        GuardClause.ValidateZeroOrNegativeId(id);

        var brand = context.Brands.Find(id);

        if (brand == null)
            throw new KeyNotFoundException($"The brand with ID {id} was not found.");

        return brand;
    }

    public List<Brand> SearchByName(string name)
    {
        GuardClause.ValidateNullOrEmptyName(name);

        return context.Brands
            .Where(b => b.Name.ToLower().Contains(name.ToLower()))
            .ToList();
    }

    public List<Brand> GetAll()
    {
        return context.Brands.ToList();
    }
}