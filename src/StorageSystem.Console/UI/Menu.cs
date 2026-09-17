using Spectre.Console;

namespace StorageSystem.Console.UI;

public class Menu(BrandMenu brandMenu, ProductMenu productMenu)
{

    public async Task Run()
    {
        bool exit = false;

        while (!exit)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Storage System").Centered().Color(Color.Orange3));
            AnsiConsole.WriteLine();

            string? option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("MAIN MENU")
                    .AddChoices(new[] { "Manage Brands", "Manage Products", "Exit" }));

            switch (option)
            {
                case "Manage Brands":
                    await brandMenu.Run();
                    break;
                case "Manage Products":
                    await productMenu.Run();
                    break;
                case "Exit":
                    exit = true;
                    AnsiConsole.Clear();
                    break;
            }
        }
    }
}