using Spectre.Console;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.View;

public enum TextUIOptions
{
    DateOnly,
    DateOnlyOptional,
    TimeSpanOnly,
    TimeSpanOnlyOptional,
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
        .MoreChoicesText("[Grey]Move up and down to reveal more options[/]")
        .UseConverter((string n) => Regex.Replace(Regex.Replace(n, @"([A-Z])", @" $1"), @"(Optional)", @"[Grey]($1)[/]"))
        .AddChoices((rawOptions)));

        int enumCardinal = (int)Enum.Parse(options, userOption);

        return enumCardinal;
    }

    /// <summary>
    /// This UI is used to assign multiple string selections that are required for the next screen, for example, while creating a new record for a database.
    /// </summary>
    /// /// <param name="title">Message that appears on the top of the selection screen.</param>
    /// <param name="options">Enum that represents selection options to appear.</param>
    /// <param name="highlightcolor">Highlight color of currently selected option.</param>
    /// /// <param name="typeDictionary">Key: Enum element from selected type in string. Value: variable in string format which user is assigning enum element to.</param>
    /// /// <param name="finalOptionName">If all non-optional parameters are inserted by the user, display additional selection option on the top with this title.</param>
    /// /// <param name="blockFinalOption">If true, it will block final option from appearing, even if all non-optional parameters have been inserted by the user.</param>
    /// /// /// <param name="blockingMessage">Message that will be displayed instead of title if final option is blocked.</param>
    /// <returns>Returns selected option for enumerator's type in int format.</returns>
    public static int? DisplaySelectionUIWithUserInputs(string title, Type options, Color highlightcolor, Dictionary<string, string> typeDictionary, string finalOptionName, bool blockFinalOption, string blockingMessage = null)
    {
        SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>();
        List<string> rawOptions = Enum.GetNames(options).ToList();

        if (string.IsNullOrEmpty(blockingMessage))
        {
            blockingMessage = title;
        }

        bool finalChoiceAvailabe = true;
        foreach (string s in rawOptions)
        {
            if (!s.Contains("Optional") && typeDictionary.ContainsKey(s))
            {
                if (string.IsNullOrEmpty(typeDictionary[s]))
                {
                    finalChoiceAvailabe = false;
                    break;
                }
            }
        }
        if (finalChoiceAvailabe && !blockFinalOption)
        {
            selectionPrompt.AddChoice(finalOptionName);
        }

        selectionPrompt
        .Title(blockFinalOption ? blockingMessage : title)
        .HighlightStyle(new Style()
            .Foreground(highlightcolor)
            .Decoration(Decoration.RapidBlink))
        
        .EnableSearch()
        .PageSize(10)
        .MoreChoicesText("[Grey]Move up and down to reveal more options[/]")
        .UseConverter((string n) => (Regex.Replace(Regex.Replace(n, @"([A-Z])", @" $1"), @"(Optional)", @"[Grey]($1)[/]") + (typeDictionary.ContainsKey(n) ? (typeDictionary[n] == null ? "" : ": [Blue]" + typeDictionary[n] + "[/]") : "")))
        .AddChoices((rawOptions));

        string userOption = AnsiConsole.Prompt(selectionPrompt);

        dynamic enumCardinal;
        bool parseSuccessful = Enum.TryParse(options, userOption, out enumCardinal);

        return parseSuccessful ? (int)enumCardinal : -1;
    }

    public static string? DisplayTextUI(string title, TextUIOptions UIOptions)
    {
        TextPrompt<string> prompt = new(title);

        if (UIOptions == TextUIOptions.Optional || UIOptions == TextUIOptions.NumbersOnlyOptional || UIOptions == TextUIOptions.DateOnlyOptional || UIOptions == TextUIOptions.TimeSpanOnlyOptional)
        {
            prompt.AllowEmpty();
        }

        if (UIOptions == TextUIOptions.NumbersOnly || UIOptions == TextUIOptions.NumbersOnlyOptional)
        {
            prompt.Validate((s) => s switch
            {
                ("") => ValidationResult.Success(),
                string when (int.TryParse(s, out _)) => ValidationResult.Success(),
                (_) => ValidationResult.Error("Your insert needs to represent an integer."),
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
        if (UIOptions == TextUIOptions.TimeSpanOnly || UIOptions == TextUIOptions.TimeSpanOnlyOptional)
        {
            prompt.Validate((s) => s switch
            {
                "" => ValidationResult.Success(),
                string when TimeSpan.TryParseExact(s,@"d\ hh\:mm",new CultureInfo("en-GB"), TimeSpanStyles.None, out _) => ValidationResult.Success(),
                _ => ValidationResult.Error("The time span you have given is not in \"d hh:mm\" format. Please try again."),
            });
        }

        return AnsiConsole.Prompt(prompt);
    }

    public static bool DisplayConfirmationSelection(string title, string positive, string negative)
    {
        System.Console.Clear();
        bool addAnotherRecord = AnsiConsole.Prompt(
            new TextPrompt<bool>(title)
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(false)
            .WithConverter(choice => choice ? positive : negative));
        return false;
    }
}
