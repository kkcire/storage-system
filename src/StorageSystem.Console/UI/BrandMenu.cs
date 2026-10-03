using StorageSystem.Application.Services;
using StorageSystem.Domain.Entities;
using Spectre.Console;

namespace StorageSystem.Console.UI;

public class BrandMenu(BrandService brandService)
{
    public async Task Run()
    {
        bool exit = false;

        while (!exit)
        {
            AnsiConsole.Clear();

            string option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold underline]BRAND MENU[/]")
                    .AddChoices("Register", "Update", "Delete", "Search by ID", "Search by Name", "List All", "Back to Main Menu"));

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
            }
        }

        AnsiConsole.Clear();
    }

    public async Task Register()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]brand name[/]:"));

        await HandleActionAsync(async () =>
        {
            Brand brand = await brandService.CreateAsync(name, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Brand '{brand.Name}' added successfully.[/]");
        }, "Registering brand...");
    }

    public async Task Update()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]brand ID[/] to update:"));
        string newName = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]new brand name[/]:"));

        await HandleActionAsync(async () =>
        {
            Brand updatedBrand = await brandService.UpdateAsync(id, newName, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Brand ID {updatedBrand.Id} updated to '{updatedBrand.Name}'.[/]");
        }, "Updating brand...");
    }

    public async Task Delete()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]brand ID[/] to delete:"));

        await HandleActionAsync(async () =>
        {
            var deletedBrand = await brandService.DeleteAsync(id, CancellationToken.None);
            AnsiConsole.MarkupLine($"[green]Brand '{deletedBrand.Name}' deleted successfully.[/]");
        }, "Deleting brand...");
    }

    public async Task SearchById()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]brand ID[/] to search:"));

        await HandleActionAsync(async () =>
        {
            Brand brand = await brandService.GetByIdAsync(id, CancellationToken.None);
            PrintBrandTable([brand]);
        }, "Searching...");
    }

    public async Task SearchByName()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]brand name[/] to search:"));

        await HandleActionAsync(async () =>
        {
            List<Brand> brands = await brandService.SearchByNameAsync(name, CancellationToken.None);
            if (brands.Count == 0)
                AnsiConsole.MarkupLine("[yellow]No brands found.[/]");
            else
                PrintBrandTable(brands);
        }, "Searching...");
    }

    public async Task ListAll()
    {
        await HandleActionAsync(async () =>
        {
            List<Brand> brands = await brandService.GetAllAsync(CancellationToken.None);
            if (brands.Count == 0)
                AnsiConsole.MarkupLine("[yellow]No brands available.[/]");
            else
                PrintBrandTable(brands);
        }, "Loading brands...");
    }

    private static void PrintBrandTable(List<Brand> brands)
    {
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Name");

        foreach (var brand in brands)
            table.AddRow(brand.Id.ToString(), brand.Name);

        AnsiConsole.Write(table);
    }

    private static async Task HandleActionAsync(Func<Task> action, string processingMessage = "Processing...")
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
}