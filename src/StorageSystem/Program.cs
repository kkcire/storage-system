using StorageSystem.Data;
using StorageSystem.Services;

using StorageContext context = new();

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