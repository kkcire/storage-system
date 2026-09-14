using Spectre.Console;
using StorageSystem.Services;
using StorageSystem.Entities;

namespace StorageSystem.UI;

public class BrandMenu(BrandService brandService)
{
    public void Run()
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
                case "Register": Register(); break;
                case "Update": Update(); break;
                case "Delete": Delete(); break;
                case "Search by ID": SearchById(); break;
                case "Search by Name": SearchByName(); break;
                case "List All": ListAll(); break;
                case "Back to Main Menu": exit = true; break;
            }
        }

        AnsiConsole.Clear();
    }

    public void Register()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]brand name[/]:"));

        HandleAction(() =>
        {
            Brand brand = brandService.Register(name);
            AnsiConsole.MarkupLine($"[green]Brand '{brand.Name}' added successfully.[/]");
        }, "Registering brand...");
    }

    public void Update()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]brand ID[/] to update:"));
        string newName = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]new brand name[/]:"));

        HandleAction(() =>
        {
            Brand updatedBrand = brandService.Update(id, newName);
            AnsiConsole.MarkupLine($"[green]Brand ID {updatedBrand.Id} updated to '{updatedBrand.Name}'.[/]");
        }, "Updating brand...");
    }

    public void Delete()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]brand ID[/] to delete:"));

        HandleAction(() =>
        {
            string deletedName = brandService.Delete(id);
            AnsiConsole.MarkupLine($"[green]Brand '{deletedName}' deleted successfully.[/]");
        }, "Deleting brand...");
    }

    public void SearchById()
    {
        int id = AnsiConsole.Prompt(new TextPrompt<int>("Enter the [green]brand ID[/] to search:"));

        HandleAction(() =>
        {
            Brand brand = brandService.GetById(id);
            PrintBrandTable([brand]);
        }, "Searching...");
    }

    public void SearchByName()
    {
        string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]brand name[/] to search:"));

        HandleAction(() =>
        {
            List<Brand> brands = brandService.SearchByName(name);
            if (brands.Count == 0)
                AnsiConsole.MarkupLine("[yellow]No brands found.[/]");
            else
                PrintBrandTable(brands);
        }, "Searching...");
    }

    public void ListAll()
    {
        HandleAction(() =>
        {
            List<Brand> brands = brandService.GetAll();
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

    private static void HandleAction(Action action, string processingMessage = "Processing...")
    {
        try
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start(processingMessage, ctx =>
                {
                    Thread.Sleep(1600);
                    action();
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