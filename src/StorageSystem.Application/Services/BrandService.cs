using StorageSystem.Application.Repository;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace StorageSystem.Application.Services;

public class BrandService(IBrandRepository repository)
{

    public async Task<Brand> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var brand = Brand.Create(name);

        if (await repository.ExistsBrandByNameAsync(brand.Name, cancellationToken))
            throw new DomainException($"A brand with the name '{brand.Name}' already exists.");

        await repository.AddAsync(brand, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return brand;
    }

    public async Task<Brand> UpdateAsync(int id, string newName, CancellationToken cancellationToken)
    {
        var brand = await repository.GetByIdAsync(id, cancellationToken);
        EnsureBrandIsNotNull(brand, id);

        string trimmedName = newName.Trim();

        if (await repository.ExistsBrandByNameAsync(trimmedName, ignoreId: brand.Id, cancellationToken))
            throw new DomainException($"A brand with the name '{trimmedName}' already exists.");

        brand.Rename(trimmedName);

        await repository.SaveChangesAsync(cancellationToken);
        return brand;
    }

    public async Task<Brand> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var brand = await repository.GetByIdAsync(id, cancellationToken);
        EnsureBrandIsNotNull(brand, id);

        if (await repository.HasProductsAsync(brand.Id, cancellationToken))
            throw new DomainException($"Cannot delete brand '{brand.Name}' because it is associated with existing products.");

        repository.Remove(brand);
        await repository.SaveChangesAsync(cancellationToken);

        return brand;
    }

    public async Task<Brand> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var brand = await repository.GetByIdAsync(id, cancellationToken);
        EnsureBrandIsNotNull(brand, id);

        return brand;
    }

    public async Task<List<Brand>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }

    public async Task<List<Brand>> SearchByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Search name cannot be empty.");

        return await repository.SearchByNameAsync(name, cancellationToken);
    }

    private static void EnsureBrandIsNotNull([NotNull] Brand? brand, int id)
    {
        if (brand is null)
            throw new KeyNotFoundException($"The brand with ID {id} was not found.");
    }

}