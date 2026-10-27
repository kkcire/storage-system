

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
                    .AddChoices("Register", "Update", "Delete", "Search by ID", "Search by Name", "List All", "Back"));

            AnsiConsole.WriteLine();

            switch (option)
            {
                case "Register": await Create(); break;
                case "Update": await updateMenu.Run(); break;
                case "Delete": await Delete(); break;
                case "Search by ID": await SearchById(); break;
                case "Search by Name": await SearchByName(); break;
                case "List All": await ListAll(); break;
                case "Back": exit = true; break;
            }
        }

        AnsiConsole.Clear();
    }

    private async Task Create()
    {
        var data = new Dictionary<string, string>();
        string title = "Registering Product";
    
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
        data["Brand ID"] = brandId.ToString();

        await Helpers.HandleActionAsync(async () =>
        {
            var product = await productService.CreateAsync(name, price, quantity, brandId, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Product '{Markup.Escape(product.Name)}' registered successfully with ID {product.Id}.[/]");
        }, "Registering product...");
    }

    private async Task Delete()
    {
        var data = new Dictionary<string, string>();
        string title = "Deleting Product";

        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID to delete:"));
        data["Product ID"] = id.ToString();

        bool confirmed = AnsiConsole.Confirm($"Delete product {id}? This action cannot be undone.");

        if (!confirmed)
        {
            AnsiConsole.MarkupLine("[yellow]Deletion canceled.[/]");
            return;
        }

        Helpers.ShowPanel(title, data);

        await Helpers.HandleActionAsync(async () =>
        {
            var deletedProduct = await productService.DeleteAsync(id, CancellationToken.None);
            data["Name"] = deletedProduct.Name;

            Helpers.ShowPanel("Deleted Product", data);
            AnsiConsole.MarkupLine($"[green]Product deleted successfully. [/]");

        }, "Deleting product...");
    }

    private async Task SearchById()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the product ID to search:"));

        await Helpers.HandleActionAsync(async () =>
        {
            Product product = await productService.GetByIdAsync(id, CancellationToken.None);
            Helpers.PrintProductTable([product]);

        }, "Searching...");
    }

    private async Task SearchByName()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the product name to search:")
            .Validate(n => !string.IsNullOrWhiteSpace(n)));

        await Helpers.HandleActionAsync(async () =>
        {
            List<Product> products = await productService.SearchByNameAsync(name, CancellationToken.None);
            if (products.Count == 0)
                AnsiConsole.MarkupLine($"[yellow]No products found with the name '{Markup.Escape(name)}'.[/]");
            else
                Helpers.PrintProductTable(products);
        }, "Searching...");
    }

    private async Task ListAll()
    {
        await Helpers.HandleActionAsync(async () =>
        {
            List<Product> products = await productService.GetAllAsync(CancellationToken.None);
            if (products.Count == 0)
                AnsiConsole.MarkupLine("[yellow]No products available.[/]");
            else
                Helpers.PrintProductTable(products);
        }, "Loading products...");
    }
}