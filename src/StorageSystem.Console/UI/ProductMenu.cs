using StorageSystem.Domain.Services;
using StorageSystem.Domain.Entities;
using Spectre.Console;

namespace StorageSystem.Console.UI;

public class ProductMenu(ProductService productService)
{
    public async Task Run()
    {
        bool exit = false;

        while (!exit)
        {
            AnsiConsole.Clear();

            string option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold underline]PRODUCT MENU[/]")
                    .AddChoices("Register", "Update", "Delete", "Search by ID", "Search by Name", "List All", "Adjust Stock", "Back to Main Menu"));

            AnsiConsole.WriteLine();

            switch (option)
            {
                case "Register": await Register(); break;
                case "Update": await Update(); break;
                case "Delete": await Delete(); break;
                case "Search by ID": await SearchById(); break;
                case "Search by Name": await SearchByName(); break;
                case "List All": await ListAll(); break;
                case "Back to Main Menu": exit = true; break;
                case "Adjust Stock": await AdjustStock(); break;
            }
        }

        AnsiConsole.Clear();
    }

    public async Task Register()
    {
        var data = new Dictionary<string, string>();
        string title = "Registering Product";

        ShowPanel(title, data);
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the product name:")
            .Validate(n => !string.IsNullOrWhiteSpace(n)));
        data["Name"] = name;

        ShowPanel(title, data);
        decimal price = AnsiConsole.Prompt(new TextPrompt<decimal>("Enter the product price:"));
        data["Price"] = price.ToString("C");

        ShowPanel(title, data);
        int quantity = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product quantity:"));
        data["Quantity"] = quantity.ToString();

        ShowPanel(title, data);
        int brandId = AnsiConsole.Prompt(new TextPrompt<int>("Enter the brand ID for the product:"));
        data["BrandId"] = brandId.ToString();

        await HandleAction(async () =>
        {
            var product = await productService.Register(name, price, quantity, brandId);
            AnsiConsole.MarkupLine($"[green]Product '{product.Name}' registered successfully with ID {product.Id}.[/]");
        }, "Registering product...");
    }

    public async Task Update()
    {
        var data = new Dictionary<string, string>();
        string title = "Updating Product";

        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID to update:"));
        data["Product ID"] = id.ToString();

        ShowPanel(title, data);
        string? name = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the new product name (or leave empty to keep current):")
                .AllowEmpty());
        name = string.IsNullOrWhiteSpace(name) ? null : name;
        data["New Name"] = name ?? "No Change";

        ShowPanel(title, data);
        decimal? price = AnsiConsole.Prompt(
            new TextPrompt<decimal?>("Enter the new product price (or leave empty to keep current):")
                .AllowEmpty());
        data["New Price"] = price?.ToString("C") ?? "No Change";

        ShowPanel(title, data);
        int? quantity = AnsiConsole.Prompt(
            new TextPrompt<int?>("Enter the new product quantity (or leave empty to keep current):")
                .AllowEmpty());
        data["New Quantity"] = quantity?.ToString() ?? "No Change";

        await HandleAction(async () =>
        {
            var updatedProduct = await productService.Update(id, name, price, quantity);
            AnsiConsole.MarkupLine($"[green]Product '{updatedProduct.Name}' updated successfully.[/]");
        }, "Updating product...");
    }

    public async Task Delete()
    {
        var data = new Dictionary<string, string>();
        string title = "Deleting Product";

        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID to delete:"));
        data["Product ID"] = id.ToString();
        ShowPanel(title, data);

        await HandleAction(async () =>
        {
            Product deletedProduct = await productService.Delete(id);
            AnsiConsole.MarkupLine($"[green]Product '{deletedProduct.Name}' deleted successfully.[/]");
        }, "Deleting product...");
    }

    public async Task SearchById()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID to search:"));

        await HandleAction(async () =>
        {
            Product product = await productService.GetById(id);
            PrintProductTable([product]);
        }, "Searching...");
    }

    public async Task SearchByName()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the product name to search:"));

        await HandleAction(async () =>
        {
            List<Product> products = await productService.SearchByName(name);
            if (products.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]No products found with the name '{name}'.[/]");
            else
                PrintProductTable(products);
        }, "Searching...");
    }

    public async Task ListAll()
    {
        await HandleAction(async () =>
        {
            List<Product> products = await productService.GetAll();
            if (products.Count == 0)
                AnsiConsole.MarkupLine("[yellow]No products available.[/]");
            else
                PrintProductTable(products);
        }, "Loading products...");
    }

    public async Task AdjustStock()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID:"));
        int amount = AnsiConsole.Prompt(new TextPrompt<int>("Enter the amount (use negative to remove stock):"));

        await HandleAction(async () =>
        {
            var product = await productService.AdjustStockQuantity(id, amount);
            AnsiConsole.MarkupLine($"[green]Stock adjusted. '{product.Name}' now has {product.Quantity} units.[/]");
        }, "Adjusting stock...");
    }

    private static void PrintProductTable(List<Product> products)
    {
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Name");
        table.AddColumn("Price");
        table.AddColumn("Quantity");
        table.AddColumn("Brand ID");

        foreach (var product in products)
            table.AddRow(product.Id.ToString(), product.Name, product.Price.ToString("C"), product.Quantity.ToString(), product.BrandId.ToString());

        AnsiConsole.Write(table);
    }

    private static async Task HandleAction(Func<Task> action, string processingMessage = "Processing...")
    {
        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(processingMessage, async ctx =>
                {
                    await action();
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Prompt(new TextPrompt<string>("[grey]Press Enter to continue...[/]").AllowEmpty());
    }

    private static void ShowPanel(string title, Dictionary<string, string> data)
    {
        AnsiConsole.Clear();

        var panel = new Panel(string.Join("\n", data.Select(kv => $"[grey]{kv.Key}:[/] [green]{kv.Value}[/]")))
            .Header($"[bold]{title}[/]")
            .Border(BoxBorder.Rounded);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }
}