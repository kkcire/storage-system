using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using StorageSystem.Application.Repository;
using StorageSystem.Application.Services;
using StorageSystem.Console.UI;
using StorageSystem.Infrastructure.Data;
using StorageSystem.Infrastructure.Repository;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found. " +
        "Configure it in the StorageSystem.Console User Secrets " +
        "or in the ConnectionStrings__DefaultConnection environment variable.");
}

var options = new DbContextOptionsBuilder<StorageContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new StorageContext(options);

try
{
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine("[red]Could not connect to or migrate the database.[/]");
    AnsiConsole.MarkupLine($"[grey]{Markup.Escape(ex.Message)}[/]");
    AnsiConsole.MarkupLine("[yellow]Is the SQL Server container running? Try: docker start sqlserver[/]");
    Environment.ExitCode = 1;
    return;
}

IBrandRepository brandRepository = new BrandRepository(context);
IProductRepository productRepository = new ProductRepository(context);

var brandService = new BrandService(brandRepository);
var productService = new ProductService(productRepository);

var brandMenu = new BrandMenu(brandService);
var updateMenu = new UpdateMenu(productService);
var productMenu = new ProductMenu(productService, updateMenu);

var menu = new Menu(brandMenu, productMenu);

await menu.Run();