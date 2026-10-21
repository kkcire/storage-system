using StorageSystem.Application.Services;
using StorageSystem.Application.Repository;
using StorageSystem.Infrastructure.Data;
using StorageSystem.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Console.UI;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

var options = new DbContextOptionsBuilder<StorageContext>()
    .UseSqlServer(connectionString)
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