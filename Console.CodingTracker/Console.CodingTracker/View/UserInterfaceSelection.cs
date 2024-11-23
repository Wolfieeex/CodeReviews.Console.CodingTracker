using Spectre.Console;

namespace Console.CodingTracker;

internal class UserInterfaceSelection : UserInterface
{
    public string DisplayUI(string title, List<string> options)
    {
        string userOption = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("What's your [green]favorite fruit[/]?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
        .AddChoices(new[] {
            "Apple", "Apricot", "Avocado", 
            "Banana", "Blackcurrant", "Blueberry",
            "Cherry", "Cloudberry", "Cocunut",
        }));

    return userOption;
    }
}