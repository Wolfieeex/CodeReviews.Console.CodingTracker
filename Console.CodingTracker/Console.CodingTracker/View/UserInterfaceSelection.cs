using Spectre.Console;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.View;

internal class UserInterfaceSelection : UserInterface
{
    public override int? DisplayUI(string title, Type options, Color highlightcolor)
    {
        List<string> rawOptions = Enum.GetNames(options).ToList();

        string userOption = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title(title)
        .HighlightStyle(new Style().Foreground(highlightcolor)
                                   .Decoration(Decoration.RapidBlink))
        .EnableSearch()
        .PageSize(10)
        .MoreChoicesText("[grey]Move up and down to reveal more options[/]")
        .UseConverter(n => Regex.Replace(n, @"([A-Z])", @" $1"))
        .AddChoices((rawOptions)));

        int enumCardinal = (int)Enum.Parse(options, userOption);
        
        return enumCardinal;
    }
}