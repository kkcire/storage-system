using StorageSystem.Application.Repository;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace StorageSystem.Application.Services;

public class ProductService(IProductRepository repository)
{
    public async Task<Product> CreateAsync(string name, decimal price, int quantity, int brandId, CancellationToken cancellationToken)
    {
        if (!await repository.BrandExistsAsync(brandId, cancellationToken))
            throw new DomainException($"The brand with ID {brandId} does not exist.");

        var product = Product.Create(name, price, quantity, brandId);

        if (await repository.ExistsProductByNameAsync(product.Name, cancellationToken))
            throw new DomainException($"A product with the name '{product.Name}' already exists.");

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(int id, string newName, decimal newPrice, int newQuantity, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        EnsureProductIsNotNull(product, id);

        string trimmedName = newName.Trim();

        if (await repository.ExistsProductByNameAsync(trimmedName, ignoreId: product.Id, cancellationToken))
            throw new DomainException($"A product with the name '{trimmedName}' already exists.");

        product.Update(trimmedName, newPrice, newQuantity);

        await repository.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> PatchAsync(int id, string? newName, decimal? newPrice, int? newQuantity, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        EnsureProductIsNotNull(product, id);

        if (newName is not null)
        {
            string trimmedName = newName.Trim();

            if (await repository.ExistsProductByNameAsync(trimmedName, ignoreId: product.Id, cancellationToken))
                throw new DomainException($"A product with the name '{trimmedName}' already exists.");

            product.Rename(trimmedName);
        }

        if (newPrice.HasValue)
            product.ChangePrice(newPrice.Value);

        if (newQuantity.HasValue)
            product.ChangeQuantity(newQuantity.Value);

        await repository.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        EnsureProductIsNotNull(product, id);

        repository.Remove(product);
        await repository.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<Product> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        EnsureProductIsNotNull(product, id);

        return product;
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }

    public async Task<List<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Search name cannot be empty.");

        return await repository.SearchByNameAsync(name, cancellationToken);
    }

    public async Task<Product> AddStockAsync(int productId, int amount, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(productId, cancellationToken);
        EnsureProductIsNotNull(product, productId);

        product.AddStock(amount);

        await repository.SaveChangesAsync(cancellationToken);
        return product;
    }

    private void EnsureProductIsNotNull([NotNull] Product? product, int id)
    {
        if (product is null)
            throw new KeyNotFoundException($"The product with ID {id} was not found.");
    }

}