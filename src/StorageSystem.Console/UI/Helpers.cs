namespace StorageSystem.Console.UI;

public class Helpers
{
    public static void ShowPanel(string title, Dictionary<string, string> data)
    {
        AnsiConsole.Clear();

        var panel = new Panel(string.Join("\n", data.Select(kv => $"[grey]{kv.Key}:[/] [green]{kv.Value}[/]")))
            .Header($"[bold]{title}[/]")
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