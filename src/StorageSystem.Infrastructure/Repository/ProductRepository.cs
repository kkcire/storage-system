using Microsoft.EntityFrameworkCore;
using StorageSystem.Application.Repository;
using StorageSystem.Infrastructure.Data;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Infrastructure.Repository;

public class ProductRepository(StorageContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
        => await context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Product>> GetAllAsync(CancellationToken ct)
        => await context.Products
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<Product>> SearchByNameAsync(string name, CancellationToken ct)
        => await context.Products
            .AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct)
        => await context.Products.AddAsync(product, ct);

    public async Task<bool> ExistsProductByNameAsync(string name, CancellationToken ct)
        => await context.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower(), ct);
    public async Task<bool> ExistsProductByNameAsync(string name, int ignoreId, CancellationToken ct)
        => await context.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower() && p.Id != ignoreId, ct);

    public async Task<bool> BrandExistsAsync(int brandId, CancellationToken ct)
        => await context.Brands.AnyAsync(b => b.Id == brandId, ct);

    public void Remove(Product product)
        => context.Products.Remove(product);

    public async Task SaveChangesAsync(CancellationToken ct)
        => await context.SaveChangesAsync(ct);

}