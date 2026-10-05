using StorageSystem.Application.Services;
using Spectre.Console;

namespace StorageSystem.Console.UI;

public class UpdateMenu(ProductService productService)
{
    public async Task Run()
    {
        bool exit = false;

        while (!exit)
        {
            AnsiConsole.Clear();

            string option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold underline]UPDATE MENU[/]")
                    .AddChoices("Full Update", "Partial Update", "Add Stock", "Change Brand", "Back to Product Menu"));

            AnsiConsole.WriteLine();

            switch (option)
            {
                case "Full Update": await FullUpdate(); break;
                case "Partial Update": await PartialUpdate(); break;
                case "Add Stock": await AddStock(); break;
                case "Change Brand": await ChangeBrand(); break;
                case "Back to Product Menu": exit = true; break;
            }
        }

        AnsiConsole.Clear();
    }

    private async Task FullUpdate()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]product ID[/] to update:"));
        var product = await productService.GetByIdAsync(id, CancellationToken.None);

        if (product == null)
        {
            AnsiConsole.MarkupLine($"[red]Product with ID {id} not found.[/]");
            return;
        }

        var data = new Dictionary<string, string>
        {
            ["Current Name"] = product.Name,
            ["Current Price"] = product.Price.ToString("C"),
            ["Current Quantity"] = product.Quantity.ToString(),
            ["Current Brand ID"] = product.BrandId.ToString()
        };

        string title = $"Updating Product ID {id}";

        Helpers.ShowPanel(title, data);

        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the new product name:"));
        data["New Name"] = name;

        Helpers.ShowPanel(title, data);

        decimal price = AnsiConsole.Prompt(new TextPrompt<decimal>("Enter the new product price:"));
        data["New Price"] = price.ToString("C");

        Helpers.ShowPanel(title, data);

        int quantity = AnsiConsole.Prompt(new TextPrompt<int>("Enter the new product quantity:"));
        data["New Quantity"] = quantity.ToString();

        Helpers.ShowPanel(title, data);

        int brandId = AnsiConsole.Prompt(new TextPrompt<int>("Enter the new brand ID for the product:"));
        data["New Brand ID"] = brandId.ToString();

        Helpers.ShowPanel(title, data);

        await Helpers.HandleActionAsync(async () =>
        {
            var updatedProduct = await productService.PatchAsync(id, name, price, quantity, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Product ID {updatedProduct.Id} updated successfully.[/]");
        }, "Updating product...");
    }

    private async Task PartialUpdate()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]product ID[/] to update:"));
        var product = await productService.GetByIdAsync(id, CancellationToken.None);

        if (product == null)
        {
            AnsiConsole.MarkupLine($"[red]Product with ID {id} not found.[/]");
            return;
        }

        var data = new Dictionary<string, string>
        {
            ["Current Name"] = product.Name,
            ["Current Price"] = product.Price.ToString("C"),
            ["Current Quantity"] = product.Quantity.ToString(),
            ["Current Brand ID"] = product.BrandId.ToString()
        };

        string title = $"Updating Product ID {id}";

        Helpers.ShowPanel(title, data);

        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the new product name (leave blank to keep current):")
            .AllowEmpty());
        data["New Name"] = string.IsNullOrWhiteSpace(name) ? "Unchanged" : name;

        Helpers.ShowPanel(title, data);

        decimal? price = AnsiConsole.Prompt(new TextPrompt<decimal?>("Enter the new product price (leave blank to keep current):")
            .AllowEmpty());
        data["New Price"] = price.HasValue ? price.Value.ToString("C") : "Unchanged";

        Helpers.ShowPanel(title, data);

        int? quantity = AnsiConsole.Prompt(new TextPrompt<int?>("Enter the new product quantity (leave blank to keep current):")
            .AllowEmpty());
        data["New Quantity"] = quantity.HasValue ? quantity.Value.ToString() : "Unchanged";

        Helpers.ShowPanel(title, data);

        await Helpers.HandleActionAsync(async () =>
        {
            var updatedProduct = await productService.PatchAsync(id, name, price, quantity, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Product ID {updatedProduct.Id} updated successfully.[/]");
        }, "Updating product...");
    }

    private async Task AddStock()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]product ID[/] to add stock:"));
        int amount = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]amount[/] to add (use negative to remove stock):"));

        await Helpers.HandleActionAsync(async () =>
        {
            var product = await productService.AddStockAsync(id, amount, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Stock adjusted. '{product.Name}' now has {product.Quantity} units.[/]");
        }, "Adjusting stock...");
    }

    private async Task ChangeBrand()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]product ID[/] to change brand:"));
        int newBrandId = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]new brand ID[/]:"));

        await Helpers.HandleActionAsync(async () =>
        {
            var product = await productService.ChangeBrandAsync(id, newBrandId, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Product '{product.Name}' now belongs to brand ID {product.BrandId}.[/]");
        }, "Changing brand...");
    }
}