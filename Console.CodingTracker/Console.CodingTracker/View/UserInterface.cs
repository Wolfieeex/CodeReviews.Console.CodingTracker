using Console.CodingTracker.Controller.SQL;
using Spectre.Console;
using System.Globalization;
using System.Text.RegularExpressions;
using Console.CodingTracker.Model;

namespace Console.CodingTracker.View;

public enum TextUIOptions
{
    DateOnly,
    DateOnlyOptional,
    StartDate,
    EndDate,
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
        .WrapAround(true)
        .HighlightStyle(new Style().Foreground(highlightcolor)
                                   .Decoration(Decoration.RapidBlink))
        .EnableSearch()
        .PageSize(15)
        .MoreChoicesText("[dim]Move up and down to reveal more options[/]")
        .UseConverter((string n) => Regex.Replace(Regex.Replace(n, @"([A-Z])", @" $1"), @"(Optional)", @$"[dim]($1)[/]"))
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
    public static int? DisplaySelectionUIWithUserInputs(string title, Type options, Color titleColor, Color highlightcolor, Color inputColor, Dictionary<string, string> typeDictionary, string finalOptionName, bool blockFinalOption, string blockingMessage = null)
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

        string hexColor = inputColor.ToHex();
        if (finalChoiceAvailabe && !blockFinalOption)
        {
            selectionPrompt.AddChoice(finalOptionName);
        }

        selectionPrompt
        .Title(blockFinalOption ? blockingMessage : title)
        .WrapAround(true)
        .HighlightStyle(new Style()
            .Foreground(highlightcolor)
            .Decoration(Decoration.RapidBlink))
        .EnableSearch()
        .PageSize(15)
        .MoreChoicesText($"[#{highlightcolor.Blend(Color.Grey19, 0.8f).ToHex()}]Move up and down to reveal more options[/]")
        .UseConverter((string n) => (Regex.Replace(Regex.Replace(n.Trim(), @"(?<=[a-z]+)([A-Z])(?=[a-z]+)", @" $1"), @"(Optional)", $@"[#{inputColor.Blend(Color.Grey19, 0.8f).ToHex()}]($1)[/]") + (typeDictionary.ContainsKey(n) ? (typeDictionary[n] == null || typeDictionary[n] == "" ? "" : ($": [#{hexColor}]" + typeDictionary[n] + "[/]")) : "")))
        .AddChoices((rawOptions));

        string userOption = AnsiConsole.Prompt(selectionPrompt);

        dynamic enumCardinal;
        bool parseSuccessful = Enum.TryParse(options, userOption, out enumCardinal);

        return parseSuccessful ? (int)enumCardinal : -1;
    }
    public static string? DisplayTextUI(string title, TextUIOptions UIOptions, Color titleColor, List<int> index = null)
    {
        string hexTitleColor = $"[#{titleColor.ToHex()}]";
        TextPrompt<string> prompt = new(title + $"{hexTitleColor}Leave this space blank[/] to clear the previous insert or {hexTitleColor}input \"E\"[/] to go back to filter menu: ");
        prompt.AllowEmpty();


        if (UIOptions == TextUIOptions.NumbersOnly || UIOptions == TextUIOptions.NumbersOnlyOptional)
        {
            prompt.Validate((s) => s.ToLower() switch
            { 
                ("") => ValidationResult.Success(),
                ("e") => ValidationResult.Success(),
                string when (int.TryParse(s, out _)) => ValidationResult.Success(),
                (_) => ValidationResult.Error($"Your insert needs to represent an {hexTitleColor}integer[/]."),
            });
        }
        if (UIOptions == TextUIOptions.DateOnly || UIOptions == TextUIOptions.DateOnlyOptional)
        {
            prompt.Validate((s) => s.ToLower() switch
            {
                "" => ValidationResult.Success(),
                ("e") => ValidationResult.Success(),
                string when DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out _) => ValidationResult.Success(),
                _ => ValidationResult.Error($"\nThe date and time you have given is not in {hexTitleColor}\"dd/mm/yyyy, hh:mm\" format[/]. Please try again.\n"),
            });
        }
        if (UIOptions == TextUIOptions.StartDate || UIOptions == TextUIOptions.EndDate)
        {
            List<string> datesToCheck;
            DateTime borderlineDate = DateTime.Now;
            bool firstIteration = true;
            DateTime userInput = DateTime.Now;
            try
            {
                if (UIOptions == TextUIOptions.StartDate)
                {
                    datesToCheck = Helpers.GetEndDates(index);
                    foreach (string d in datesToCheck)
                    {
                        if (firstIteration)
                        {
                            firstIteration = false;
                            DateTime.TryParseExact(d, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out borderlineDate);
                            continue;
                        }
                        DateTime currentDate = DateTime.Parse(d, new CultureInfo("en-GB"), DateTimeStyles.None);
                        if (currentDate < borderlineDate)
                        {
                            borderlineDate = currentDate;
                        }
                    }

                    prompt.Validate((s) => s.ToLower() switch
                    {
                        "" => ValidationResult.Success(),
                        ("e") => ValidationResult.Success(),
                        string when (!DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out userInput)) => ValidationResult.Error($"The date and time you have given is not in {hexTitleColor}\"dd/mm/yyyy, hh:mm\" format[/]. Please try again."),
                        string when userInput > borderlineDate => ValidationResult.Error($"\nOne or more session {hexTitleColor}end dates would fall earlier than the newly updated start date.[/] Please try again.\n"),
                        _ => ValidationResult.Success()
                    });
                }
                else
                {
                    if (UIOptions == TextUIOptions.EndDate)
                    {
                        datesToCheck = Helpers.GetStartDates(index);
                        foreach (string d in datesToCheck)
                        {
                            if (firstIteration)
                            {
                                firstIteration = false;
                                DateTime.TryParseExact(d, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out borderlineDate);
                                continue;
                            }
                            DateTime currentDate = DateTime.Parse(d, new CultureInfo("en-GB"), DateTimeStyles.None);
                            if (currentDate > borderlineDate)
                            {
                                borderlineDate = currentDate;
                            }
                        }

                        prompt.Validate((s) => s.ToLower() switch
                        {
                            "" => ValidationResult.Success(),
                            ("e") => ValidationResult.Success(),
                            string when (!DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out userInput)) => ValidationResult.Error($"The date and time you have given is not in {hexTitleColor}\"dd/mm/yyyy, hh:mm\" format[/]. Please try again."),
                            string when userInput < borderlineDate => ValidationResult.Error($"\nOne or more session {hexTitleColor}start dates would fall later than the newly updated end date.[/] Please try again.\n"),
                            _ => ValidationResult.Success()
                        });
                    }
                }
            }
            catch 
            {
                System.Console.WriteLine("DisplayTextUI method with Start or End date selector always need index numbers provided in the parameters field.");
                System.Console.ReadKey();

                prompt.Validate((s) => s.ToLower() switch
                {
                    "" => ValidationResult.Success(),
                    ("e") => ValidationResult.Success(),
                    string when DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out _) => ValidationResult.Success(),
                    _ => ValidationResult.Error($"The date and time you have given is not in {hexTitleColor}\"dd/mm/yyyy, hh:mm\" format[/]. Please try again."),
                });
            }

        }
        if (UIOptions == TextUIOptions.TimeSpanOnly || UIOptions == TextUIOptions.TimeSpanOnlyOptional)
        {
            prompt.Validate((s) => s.ToLower() switch
            {
                "" => ValidationResult.Success(),
                ("e") => ValidationResult.Success(),
                string when TimeSpan.TryParseExact(s,@"d\ hh\:mm",new CultureInfo("en-GB"), TimeSpanStyles.None, out _) => ValidationResult.Success(),
                _ => ValidationResult.Error($"The time span you have given is not in {hexTitleColor}\"d hh:mm\" format[/]. Please try again."),
            });
        }

        string userOption = AnsiConsole.Prompt(prompt);
        userOption = Regex.Replace(userOption, @"[\[\]\{\}]", "");

        return userOption;
    }
    public static bool DisplayConfirmationSelectionUI(string title, string positive, string negative, Color inputColor)
    {
        positive = positive.ToLower();

        negative = negative.ToLower();

        System.Console.Clear();
        TextPrompt<bool> prompt =
            new TextPrompt<bool>(title + $" [#{inputColor.Blend(Color.Red, 0.5f).ToHex()}]({positive}[/][#{inputColor.ToHex()}]/[/][#{inputColor.Blend(Color.Green, 0.5f).ToHex()}]{negative})[/] - [#{inputColor.Blend(Color.Green, 0.5f).ToHex()}]{negative} on default if you insert blank:[/]")
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(false)
            .HideChoices()
            .HideDefaultValue()
            .DefaultValueStyle(new Style(foreground: inputColor.Blend(Color.Green, 0.5f)))
            .WithConverter(choice => choice ? positive : negative);

        bool addAnotherRecord = AnsiConsole.Prompt(prompt);
        return addAnotherRecord;
    }
    /// <summary>
    /// Returns "0" if user goes back to previous menu. Returns "-1" if user cancels the operation.
    /// </summary>
    public static string DisplayStringListSelectionUI(string title, List<string> options, Color color)
    {
        SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>();

        selectionPrompt
        .Title(title)
        .WrapAround(true)
        .HighlightStyle(new Style()
            .Foreground(color)
            .Decoration(Decoration.RapidBlink))
        .EnableSearch()
        .PageSize(15)
        .MoreChoicesText($"[#{color.Blend(Color.Grey, 0.5f).ToHex()}]Move up and down to reveal more options[/]")
        .AddChoices("Go Back")
        .AddChoices("Cancel")
        .AddChoices(options);

        string userOption = AnsiConsole.Prompt(selectionPrompt);

        if (userOption == "Go Back")
        {
            return "0";
        }
        if (userOption == "Cancel")
        {
            return "-1";
        }

        return userOption;
    }
    public static dynamic DisplayEnumSelectionUI(string title, Type options, Color color)
    {
        SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>();
        List<string> rawOptions = Enum.GetNames(options).ToList();

        selectionPrompt
        .Title(title)
        .WrapAround(true)
        .HighlightStyle(new Style()
            .Foreground(color)
            .Decoration(Decoration.RapidBlink))
        .EnableSearch()
        .PageSize(15)
        .MoreChoicesText($"[#{color.Blend(Color.Grey, 0.5f).ToHex()}]Move up and down to reveal more options[/]")
        .AddChoices("Cancel")
        .AddChoices(rawOptions)
        .UseConverter(n => Regex.Replace(n, @"(?<=[A-Za-z])([A-Z])", @" $1"));

        string userOption = AnsiConsole.Prompt(selectionPrompt);

        dynamic enumCardinal;
        bool parseSuccessful = Enum.TryParse(options, userOption, out enumCardinal);

        return parseSuccessful ? enumCardinal : -1;
    }
    public static void DisplayMultiselectionUI(string title, Type options, ref bool[] updateArray, Color mainColor)
    {
        bool runMenu = true;
        string originalTitle = title;
        while (runMenu)
        {
            SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>();

            List<string> stringOptions = Enum.GetNames(options).ToList();

            if (stringOptions.Count != updateArray.Length)
            {
                throw new ArgumentException("For Display Multiselection UI method passed Enum Type needs to have the same length as referenced array of bools.");
            }
            for (int i = 0; i < stringOptions.Count; i++)
            {
                stringOptions[i] += ":" + (updateArray[i] ? $"[#{mainColor.Blend(Color.Green, 0.5f).ToHex()}]☑[/] " : $"[#{mainColor.Blend(Color.Grey, 0.5f).ToHex()}]☐[/] ");
            }

            selectionPrompt
            .Title(title)
            .WrapAround(true)
            .HighlightStyle(new Style()
                .Foreground(mainColor)
                .Decoration(Decoration.RapidBlink))
            .EnableSearch()
            .PageSize(15)
            .MoreChoicesText($"[#{mainColor.Blend(Color.Grey, 0.5f).ToHex()}]Move up and down to reveal more options[/]")
            .UseConverter((string n) => (Regex.Replace(n, @"(?<!^)([A-Z])(?=.*:)", @" $1")))
            .AddChoices("Save and exit")
            .AddChoices(stringOptions);


            string userOption = AnsiConsole.Prompt(selectionPrompt);
            if (userOption == "Save and exit")
            {
                runMenu = false;
            }
            else
            {
                title = originalTitle;
                userOption = Regex.Match(userOption, @".*(?=:)").Value;
                int optionCardinal = (int)Enum.Parse(options, userOption);
                updateArray[optionCardinal] = !updateArray[optionCardinal];
                
                bool allValuesFalse = true;
                foreach (bool b in updateArray)
                {
                    if (b == true)
                    {
                        allValuesFalse = false;
                        break;
                    }
                }
                if (allValuesFalse)
                {
                    title = $"[#{mainColor.Blend(Color.Red, 0.5f).ToHex()}]At least one value must be selected for your table[/]";
                    updateArray[optionCardinal] = !updateArray[optionCardinal];
                }
            }
        }
    }
}
