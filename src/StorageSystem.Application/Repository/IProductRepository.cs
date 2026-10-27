using StorageSystem.Domain.Entities;

namespace StorageSystem.Application.Repository;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Product>> GetAllAsync(CancellationToken ct);
    Task<List<Product>> SearchByNameAsync(string name, CancellationToken ct);
    Task AddAsync(Product product, CancellationToken ct);
    Task<bool> ExistsProductByNameAsync(string name, CancellationToken ct);
    Task<bool> ExistsProductByNameAsync(string name, int ignoreId, CancellationToken ct);
    Task<bool> BrandExistsAsync(int brandId, CancellationToken ct);
    void Remove(Product product);
    Task SaveChangesAsync(CancellationToken ct);
}