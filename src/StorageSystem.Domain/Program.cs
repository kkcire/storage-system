using StorageSystem.Domain.Data;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Services;
using StorageSystem.UI;

var options = new DbContextOptionsBuilder<StorageContext>()
    .UseSqlite("Data Source = storage.db")
    .Options;

StorageContext context = new(options);

BrandService brandService = new(context);
BrandMenu brandMenu = new(brandService);

ProductService productService = new(context);
ProductMenu productMenu = new(productService);

Menu menu = new(brandMenu, productMenu);

menu.Run();