using StorageSystem.Domain.Data;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Services;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Console.UI;

var dbPath = Environment.GetEnvironmentVariable("STORAGE_DB_PATH")
    ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "storage.db");

var options = new DbContextOptionsBuilder<StorageContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

StorageContext context = new(options);

BrandService brandService = new(context);
BrandMenu brandMenu = new(brandService);

ProductService productService = new(context);
ProductMenu productMenu = new(productService);

Menu menu = new(brandMenu, productMenu);

menu.Run();