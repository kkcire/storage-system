using StorageSystem.Application.Services;
using StorageSystem.Application.Repository;
using StorageSystem.Infrastructure.Data;
using StorageSystem.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using StorageSystem.Console.UI;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. Configure it in appsettings.json, user secrets, or ConnectionStrings__DefaultConnection environment variable.");

var options = new DbContextOptionsBuilder<StorageContext>()
    .UseSqlServer(connectionString)
    .Options;

using StorageContext context = new(options);
context.Database.EnsureCreated();

IBrandRepository brandRepository = new BrandRepository(context);
IProductRepository productRepository = new ProductRepository(context);


BrandService brandService = new(brandRepository);
ProductService productService = new(productRepository);

BrandMenu brandMenu = new(brandService);
UpdateMenu updateMenu = new(productService);
ProductMenu productMenu = new(productService, updateMenu);

Menu menu = new(brandMenu, productMenu);

await menu.Run();