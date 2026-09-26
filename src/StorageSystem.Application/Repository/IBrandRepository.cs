using StorageSystem.Domain.Entities;

namespace StorageSystem.Application.Repository;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Brand>> GetAllAsync(CancellationToken ct = default);
    Task<List<Brand>> SearchByNameAsync(string name, CancellationToken ct = default);
    
    Task AddAsync(Brand brand, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    void Remove(Brand brand);
    Task SaveChangesAsync(CancellationToken ct = default);
}