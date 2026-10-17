using StorageSystem.Domain.Entities;

namespace StorageSystem.Application.Repository;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Brand>> GetAllAsync(CancellationToken ct);
    Task<List<Brand>> SearchByNameAsync(string name, CancellationToken ct);
    Task AddAsync(Brand brand, CancellationToken ct);
    Task<bool> ExistsBrandByNameAsync(string name, CancellationToken ct);
    Task<bool> ExistsBrandByNameAsync(string name, int ignoreId, CancellationToken ct);

    Task<bool> HasProductsAsync(int brandId, CancellationToken ct);
    void Remove(Brand brand);
    Task SaveChangesAsync(CancellationToken ct);
}