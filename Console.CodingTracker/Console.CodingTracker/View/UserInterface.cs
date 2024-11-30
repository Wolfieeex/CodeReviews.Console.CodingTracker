using Spectre.Console;
using System.Globalization;
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

    public static int? DisplaySelectionUIWithUserInputs(string title, Type options, Color highlightcolor, Dictionary<string, string> dic, string finalOptionName)
    {
        SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>();
        List<string> rawOptions = Enum.GetNames(options).ToList();

        bool finalChoiceAvailabe = true;
        foreach (string s in rawOptions)
        {
            if (!s.Contains("Optional") && dic.ContainsKey(s))
            {
                if (string.IsNullOrEmpty(dic[s]))
                {
                    finalChoiceAvailabe = false;
                    break;
                }
            }
        }
        if (finalChoiceAvailabe)
        {
            selectionPrompt.AddChoice(finalOptionName);
        }

        selectionPrompt
        .Title(title)
        .HighlightStyle(new Style()
            .Foreground(highlightcolor)
            .Decoration(Decoration.RapidBlink))
        .EnableSearch()
        .PageSize(10)
        .MoreChoicesText("[grey]Move up and down to reveal more options[/]")
        .UseConverter((string n) => Regex.Replace(Regex.Replace(n, @"([A-Z])", @" $1"), @"(Optional)", @"[grey]〚$1〛[/]") + (dic.ContainsKey(n) ? (dic[n] == null ? "" : ": [Blue]" + dic[n] + "[/]") : ""))
        .AddChoices((rawOptions));

        string userOption = AnsiConsole.Prompt(selectionPrompt);

        dynamic enumCardinal;
        bool parseSuccessful = Enum.TryParse(options, userOption, out enumCardinal);

        return parseSuccessful ? (int)enumCardinal : -1;
    }

    public static string? DisplayTextUI(string title, TextUIOptions UIOptions)
    {
        TextPrompt<string> prompt = new(title);

        if (UIOptions == TextUIOptions.Optional || UIOptions == TextUIOptions.NumbersOnlyOptional || UIOptions == TextUIOptions.DateOnlyOptional)
        {
            prompt.AllowEmpty();
        }

        if (UIOptions == TextUIOptions.NumbersOnly || UIOptions == TextUIOptions.NumbersOnlyOptional)
        {
            prompt.Validate((s) => s switch
            {
                "" => ValidationResult.Success(),
                string when int.TryParse(s, out _) => ValidationResult.Success(),
                _ => ValidationResult.Error("Your insert needs to represent an integer."),
            });
        }
        if (UIOptions == TextUIOptions.DateOnly || UIOptions == TextUIOptions.DateOnlyOptional)
        {
            prompt.Validate((s) => s switch
            {
                "" => ValidationResult.Success(),
                string when DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out _) => ValidationResult.Success(),
                _ => ValidationResult.Error("The date and time you have given is not in \"dd/mm/yyyy, hh:mm\" format. Please try again."),
            });
        }


        return AnsiConsole.Prompt(prompt);
    }
}
