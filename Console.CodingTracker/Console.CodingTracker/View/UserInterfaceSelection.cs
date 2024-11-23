using Spectre.Console;
using Console.CodingTracker.MenuSelections;

namespace Console.CodingTracker;

internal class UserInterfaceSelection : UserInterface
{
    public string DisplayUI(string title, Type options)
    {
        string userOption = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title(title)
        .PageSize(3)
        .MoreChoicesText("Move up and down to reveal more options.")
        .AddChoices(Enum.GetNames(options)));

    return userOption;
    }
}