

namespace StorageSystem.Console.UI;

public class ProductMenu(ProductService productService, UpdateMenu updateMenu)
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
                case "Create": await Create(); break;
                case "Update": await updateMenu.Run(); break;
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

    public async Task Create()
    {
        var data = new Dictionary<string, string>();
        string title = "Registering Product";

        Helpers.ShowPanel(title, data);
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the product name:")
            .Validate(n => !string.IsNullOrWhiteSpace(n)));
        data["Name"] = name;

        Helpers.ShowPanel(title, data);
        decimal price = AnsiConsole.Prompt(new TextPrompt<decimal>("Enter the product price:"));
        data["Price"] = price.ToString("C");

        Helpers.ShowPanel(title, data);
        int quantity = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product quantity:"));
        data["Quantity"] = quantity.ToString();

        Helpers.ShowPanel(title, data);
        int brandId = AnsiConsole.Prompt(new TextPrompt<int>("Enter the brand ID for the product:"));
        data["BrandId"] = brandId.ToString();

        await Helpers.HandleActionAsync(async () =>
        {
            var product = await productService.CreateAsync(name, price, quantity, brandId, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Product '{product.Name}' registered successfully with ID {product.Id}.[/]");
        }, "Registering product...");
    }

    public async Task Delete()
    {
        var data = new Dictionary<string, string>();
        string title = "Deleting Product";

        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID to delete:"));
        data["Product ID"] = id.ToString();
        Helpers.ShowPanel(title, data);

        await Helpers.HandleActionAsync(async () =>
        {
            var deletedProduct = await productService.DeleteAsync(id, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Product '{deletedProduct.Name}' deleted successfully.[/]");
        }, "Deleting product...");
    }

    public async Task SearchById()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID to search:"));

        await Helpers.HandleActionAsync(async () =>
        {
            Product product = await productService.GetByIdAsync(id, CancellationToken.None);
            PrintProductTable([product]);
        }, "Searching...");
    }

    public async Task SearchByName()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the product name to search:"));

        await Helpers.HandleActionAsync(async () =>
        {
            List<Product> products = await productService.SearchByNameAsync(name, CancellationToken.None);
            if (products.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]No products found with the name '{name}'.[/]");
            else
                PrintProductTable(products);
        }, "Searching...");
    }

    public async Task ListAll()
    {
        await Helpers.HandleActionAsync(async () =>
        {
            List<Product> products = await productService.GetAllAsync(CancellationToken.None);
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

        await Helpers.HandleActionAsync(async () =>
        {
            var product = await productService.AddStockAsync(id, amount, CancellationToken.None);
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
        table.AddColumn("Brand");

        foreach (var product in products)
            table.AddRow(product.Id.ToString(), product.Name, product.Price.ToString("C"), product.Quantity.ToString(), product.Brand.Name);

        AnsiConsole.Write(table);
    }
}