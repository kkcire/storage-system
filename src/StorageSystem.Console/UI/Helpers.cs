namespace StorageSystem.Console.UI;

public class Helpers
{
    public static void ShowPanel(string title, Dictionary<string, string> data)
    {
        var lines = data.Select(kv =>
            $"[grey]{Markup.Escape(kv.Key)}:[/] [green]{Markup.Escape(kv.Value)}[/]");

        var panel = new Panel(string.Join("\n", lines))
            .Header($"[bold]{Markup.Escape(title)}[/]")
            .Border(BoxBorder.Rounded);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static async Task HandleActionAsync(Func<Task> action, string processingMessage = "Processing...")
    {
        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(processingMessage, async _ =>
                {
                    await action();
                });
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Operation canceled.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]Error: {Markup.Escape(ex.Message)}[/]");
        }

        AnsiConsole.WriteLine();

        AnsiConsole.Prompt(
            new TextPrompt<string>("[grey]Press Enter to continue...[/]")
                .AllowEmpty());
    }

    public static void PrintProductTable(List<Product> products)
    {
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Name");
        table.AddColumn("Price");
        table.AddColumn("Quantity");
        table.AddColumn("Brand");

        foreach (var product in products)
            table.AddRow(
                product.Id.ToString(),
                Markup.Escape(product.Name),
                product.Price.ToString("C"),
                product.Quantity.ToString(),
                Markup.Escape(product.Brand.Name));

        AnsiConsole.Write(table);
    }
}