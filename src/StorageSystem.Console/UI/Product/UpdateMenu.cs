using Spectre.Console;
using StorageSystem.Application.Services;
using StorageSystem.Domain.Entities;

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
                    .AddChoices("Full Update", "Partial Update", "Add Stock", "Change Brand", "Back"));

            AnsiConsole.WriteLine();

            switch (option)
            {
                case "Full Update": await FullUpdate(); break;
                case "Partial Update": await PartialUpdate(); break;
                case "Add Stock": await AddStock(); break;
                case "Change Brand": await ChangeBrand(); break;
                case "Back": exit = true; break;
            }
        }

        AnsiConsole.Clear();
    }

    private async Task FullUpdate()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]product ID[/] to update:"));

        Product product;
        try
        {
            product = await productService.GetByIdAsync(id, CancellationToken.None);
        }
        catch (KeyNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(ex.Message)}[/]");
            Pause();
            return;
        }

        var data = new Dictionary<string, string>
        {
            ["Current Name"] = product.Name,
            ["Current Price"] = product.Price.ToString("C"),
            ["Current Quantity"] = product.Quantity.ToString()
        };

        string title = $"Updating Product ID {id}";

        Helpers.ShowPanel(title, data);

        string name = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the new product name:")
                .Validate(n => !string.IsNullOrWhiteSpace(n)));
        data["New Name"] = name;

        Helpers.ShowPanel(title, data);

        decimal price = AnsiConsole.Prompt(new TextPrompt<decimal>("Enter the new product price:"));
        data["New Price"] = price.ToString("C");

        Helpers.ShowPanel(title, data);

        int quantity = AnsiConsole.Prompt(new TextPrompt<int>("Enter the new product quantity:"));
        data["New Quantity"] = quantity.ToString();

        Helpers.ShowPanel(title, data);

        await Helpers.HandleActionAsync(async () =>
        {
            var updatedProduct = await productService.UpdateAsync(id, name, price, quantity, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Product ID {updatedProduct.Id} updated successfully.[/]");
        }, "Updating product...");
    }

    private async Task PartialUpdate()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]product ID[/] to update:"));

        Product product;
        
        try
        {
            product = await productService.GetByIdAsync(id, CancellationToken.None);
        }
        catch (KeyNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(ex.Message)}[/]");
            Pause();
            return;
        }

        var data = new Dictionary<string, string>
        {
            ["Current Name"] = product.Name,
            ["Current Price"] = product.Price.ToString("C"),
            ["Current Quantity"] = product.Quantity.ToString()
        };

        string title = $"Updating Product ID {id}";

        Helpers.ShowPanel(title, data);

        string? name = AnsiConsole.Prompt(
            new TextPrompt<string?>("Enter the new product name (leave blank to keep current):")
                .AllowEmpty());
        name = string.IsNullOrWhiteSpace(name) ? null : name;
        data["New Name"] = name ?? "Unchanged";

        Helpers.ShowPanel(title, data);

        decimal? price = AnsiConsole.Prompt(
            new TextPrompt<decimal?>("Enter the new product price (leave blank to keep current):")
                .AllowEmpty());
        data["New Price"] = price?.ToString("C") ?? "Unchanged";

        Helpers.ShowPanel(title, data);

        int? quantity = AnsiConsole.Prompt(
            new TextPrompt<int?>("Enter the new product quantity (leave blank to keep current):")
                .AllowEmpty());
        data["New Quantity"] = quantity?.ToString() ?? "Unchanged";

        Helpers.ShowPanel(title, data);

        if (name is null && price is null && quantity is null)
        {
            AnsiConsole.MarkupLine("[yellow]Nothing to update.[/]");
            Pause();
            return;
        }

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
            AnsiConsole.MarkupLine(
                $"[green]Stock adjusted. '{Markup.Escape(product.Name)}' now has {product.Quantity} units.[/]");
        }, "Updating stock...");
    }

    private async Task ChangeBrand()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]product ID[/] to change brand:"));
        int newBrandId = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]new brand ID[/]:"));

        await Helpers.HandleActionAsync(async () =>
        {
            var product = await productService.ChangeBrandAsync(id, newBrandId, CancellationToken.None);
            AnsiConsole.MarkupLine(
                $"[green]Product '{Markup.Escape(product.Name)}' now belongs to brand ID {product.BrandId}.[/]");
        }, "Changing brand...");
    }

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Prompt(new TextPrompt<string>("[grey]Press Enter to continue...[/]").AllowEmpty());
    }
}