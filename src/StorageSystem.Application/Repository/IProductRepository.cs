using StorageSystem.Domain.Entities;

namespace StorageSystem.Application.Repository;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<List<Product>> SearchByNameAsync(string name, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);
    Task<bool> ExistsProductByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsProductByNameAsync(string name, int ignoreId, CancellationToken ct = default);
    Task<bool> BrandExistsAsync(int brandId, CancellationToken ct = default);
    void Remove(Product product);
    Task SaveChangesAsync(CancellationToken ct = default);
}