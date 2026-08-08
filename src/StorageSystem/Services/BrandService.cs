using StorageSystem.Data;
using StorageSystem.Entities;

namespace StorageSystem.Services;

public class BrandService(StorageContext context)
{
    public Brand Add(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The name of the Brand cannot be empty.");

        Brand brand = new() { Name = name };
        context.Brands.Add(brand);
        context.SaveChanges();
        return brand;
    }

    public Brand Update(int id, string newName)
    {
        if (id <= 0)
            throw new ArgumentException("The ID cannot be zero or negative.");

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("The new name cannot be empty.");

        var brand = context.Brands.FirstOrDefault(b => b.Id == id);

        if (brand == null)
            throw new KeyNotFoundException($"The brand with ID {id} was not found.");

        brand.Name = newName;
        context.SaveChanges();

        return brand;
    }

    public string Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("The ID cannot be zero or negative.");

        var brand = context.Brands.FirstOrDefault(b => b.Id == id);

        if (brand == null)
            throw new KeyNotFoundException($"The brand with ID {id} was not found.");

        context.Brands.Remove(brand);
        context.SaveChanges();

        return brand.Name;
    }

    public Brand GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("The ID cannot be zero or negative.");

        var brand = context.Brands.FirstOrDefault(b => b.Id == id);

        if (brand == null)
            throw new KeyNotFoundException($"The brand with ID {id} was not found.");

        return brand;
    }
    
    public List<Brand> SearchByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The name of the Brand cannot be empty.");

        return context.Brands.Where(b => b.Name.Contains(name)).ToList();
    }

    public List<Brand> GetAll()
    {
        return context.Brands.ToList();
    }
}