using Spectre.Console;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.View;

public enum TextUIOptions
{
    DateOnly,
    DateOnlyOptional,
    NumbersOnly,
    NumbersOnlyOptional,
    Optional,
    Any
}

internal class UserInterface
{
    public static int? DisplaySelectionUI(string title, Type options, Color highlightcolor)
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
        .UseConverter((string n) => Regex.Replace(Regex.Replace(n, @"([A-Z])", @" $1"), @"(Optional)", @"[grey]($1)[/]"))
        .AddChoices((rawOptions)));

        int enumCardinal = (int)Enum.Parse(options, userOption);

        return enumCardinal;
    }

    public static int? DisplaySelectionUIWithUserInputs(string title, Type options, Color highlightcolor, Dictionary<string, string> dic)
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
        .UseConverter((string n) => Regex.Replace(Regex.Replace(n, @"([A-Z])", @" $1"), @"(Optional)", @"[grey]($1)[/]") + (dic[n] == null ? "" : ": [green]" + dic[n] + "[/]"))
        .AddChoices((rawOptions)));

        int enumCardinal = (int)Enum.Parse(options, userOption);

        return enumCardinal;
    }

    public static string? DisplayTextUI(string title, TextUIOptions UIOptions)
    {
        TextPrompt<string> prompt = new(title);

        if (UIOptions == TextUIOptions.Optional || UIOptions == TextUIOptions.NumbersOnlyOptional)
        {
            prompt.AllowEmpty();
        }

        if (UIOptions == TextUIOptions.NumbersOnly || UIOptions == TextUIOptions.NumbersOnlyOptional)
        {
            prompt.Validate((s) => s switch
            {
                "" => ValidationResult.Success(),
                string when int.TryParse(s, out _) => ValidationResult.Success(),
                _ => ValidationResult.Error("Your insert needs to represent an integer"),
            });
        }

        return AnsiConsole.Prompt(prompt);
    }
}
