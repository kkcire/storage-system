using StorageSystem.Application.Services;
using StorageSystem.Application.Repository;
using StorageSystem.Infrastructure.Data;
using StorageSystem.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Console.UI;

var dbPath = Environment.GetEnvironmentVariable("STORAGE_DB_PATH")
    ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "storage.db");

var options = new DbContextOptionsBuilder<StorageContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

StorageContext context = new(options);

IBrandRepository brandRepository = new BrandRepository(context);
IProductRepository productRepository = new ProductRepository(context);


BrandService brandService = new(brandRepository);
ProductService productService = new(productRepository);

BrandMenu brandMenu = new(brandService);
UpdateMenu updateMenu = new(productService);
ProductMenu productMenu = new(productService, updateMenu);

Menu menu = new(brandMenu, productMenu);

await menu.Run();