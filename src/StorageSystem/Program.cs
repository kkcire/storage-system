using StorageSystem.Data;
using StorageSystem.Services;

var options = new DbContextOptionsBuilder<StorageContext>()
    .UseSqlite("Data Source = storage.db")
    .Options;

using StorageContext context = new(options);

BrandService brandService = new(context);

var brands = brandService.GetAll();

if (!brands.Any())
{
    Console.WriteLine("Nenhuma marca encontrada no banco de dados.");
}
else
{
    foreach (var brand in brands)
    {
        Console.WriteLine($"ID: {brand.Id} | Nome: {brand.Name}");
    }
}