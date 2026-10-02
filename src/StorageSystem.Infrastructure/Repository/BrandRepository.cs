using Microsoft.EntityFrameworkCore;
using StorageSystem.Infrastructure.Data;
using StorageSystem.Application.Repository;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Infrastructure.Repository;

public class BrandRepository(StorageContext context) : IBrandRepository
{
    public async Task<Brand?> GetByIdAsync(int id, CancellationToken ct)
        => await context.Brands.FirstOrDefaultAsync(brand => brand.Id == id, ct);

    public async Task<List<Brand>> GetAllAsync(CancellationToken ct)
        => await context.Brands
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<Brand>> SearchByNameAsync(string name, CancellationToken ct)
        => await context.Brands
            .AsNoTracking()
            .Where(brand => brand.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync(ct);

    public async Task AddAsync(Brand brand, CancellationToken ct)
        => await context.Brands.AddAsync(brand, ct);

    public async Task<bool> ExistsBrandByNameAsync(string name, CancellationToken ct)
        => await context.Brands.AnyAsync(brand => brand.Name.ToLower() == name.ToLower(), ct);
    public async Task<bool> ExistsBrandByNameAsync(string name, int ignoreId, CancellationToken ct)
        => await context.Brands.AnyAsync(brand => brand.Name.ToLower() == name.ToLower() && brand.Id != ignoreId, ct);

    public async Task<bool> HasProductsAsync(int brandId, CancellationToken ct = default)
        => await context.Products.AnyAsync(p => p.BrandId == brandId, ct);

    public void Remove(Brand brand)
        => context.Brands.Remove(brand);

    public async Task SaveChangesAsync(CancellationToken ct)
        => await context.SaveChangesAsync(ct);
}