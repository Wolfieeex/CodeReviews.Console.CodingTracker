using Console.CodingTracker.Controller;
using Console.CodingTracker.Model;
using Spectre.Console;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

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
        .WrapAround(true)
        .HighlightStyle(new Style()
            .Foreground(highlightcolor)
            .Decoration(Decoration.RapidBlink))
        .EnableSearch()
        .PageSize(15)
        .MoreChoicesText("[Grey]Move up and down to reveal more options[/]")
        .UseConverter((string n) => (Regex.Replace(Regex.Replace(n, @"([A-Z])", @" $1"), @"(Optional)", @"[Grey]($1)[/]") + (typeDictionary.ContainsKey(n) ? (typeDictionary[n] == null || typeDictionary[n] == "" ? "" : ": [Blue]" + typeDictionary[n] + "[/]") : "")))
        .AddChoices((rawOptions));

        string userOption = AnsiConsole.Prompt(selectionPrompt);

        dynamic enumCardinal;
        bool parseSuccessful = Enum.TryParse(options, userOption, out enumCardinal);

        return parseSuccessful ? (int)enumCardinal : -1;
    }
    public static string? DisplayTextUI(string title, TextUIOptions UIOptions, List<int> index = null)
    {
        TextPrompt<string> prompt = new(title + "[red]Leave this space blank[/] to clear the previous insert or [red]input \"E\"[/] to go back to filter menu: ");
        prompt.AllowEmpty();


        if (UIOptions == TextUIOptions.NumbersOnly || UIOptions == TextUIOptions.NumbersOnlyOptional)
        {
            prompt.Validate((s) => s.ToLower() switch
            { 
                ("") => ValidationResult.Success(),
                ("e") => ValidationResult.Success(),
                string when (int.TryParse(s, out _)) => ValidationResult.Success(),
                (_) => ValidationResult.Error("Your insert needs to represent an integer."),
            });
        }
        if (UIOptions == TextUIOptions.DateOnly || UIOptions == TextUIOptions.DateOnlyOptional)
        {
            prompt.Validate((s) => s.ToLower() switch
            {
                "" => ValidationResult.Success(),
                ("e") => ValidationResult.Success(),
                string when DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out _) => ValidationResult.Success(),
                _ => ValidationResult.Error("\nThe date and time you have given is not in [red]\"dd/mm/yyyy, hh:mm\" format[/]. Please try again.\n"),
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
                    datesToCheck = SQLCommands.GetEndDates(index);
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
                        string when (!DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out userInput)) => ValidationResult.Error("The date and time you have given is not in \"dd/mm/yyyy, hh:mm\" format. Please try again."),
                        string when userInput > borderlineDate => ValidationResult.Error("\nOne or more session [red]end dates would fall earlier than the newly updated start date.[/] Please try again.\n"),
                        _ => ValidationResult.Success()
                    });
                }
                else
                {
                    if (UIOptions == TextUIOptions.EndDate)
                    {
                        datesToCheck = SQLCommands.GetStartDates(index);
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
                            string when (!DateTime.TryParseExact(s, "dd/MM/yyyy, HH:mm", new CultureInfo("en-GB"), DateTimeStyles.None, out userInput)) => ValidationResult.Error("The date and time you have given is not in \"dd/mm/yyyy, hh:mm\" format. Please try again."),
                            string when userInput < borderlineDate => ValidationResult.Error("\nOne or more session [red]start dates would fall later than the newly updated end date.[/] Please try again.\n"),
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
                    _ => ValidationResult.Error("The date and time you have given is not in \"dd/mm/yyyy, hh:mm\" format. Please try again."),
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
                _ => ValidationResult.Error("The time span you have given is not in \"d hh:mm\" format. Please try again."),
            });
        }

        return AnsiConsole.Prompt(prompt);
    }
    public static bool DisplayConfirmationSelectionUI(string title, string positive, string negative)
    {
        positive = positive.ToLower();

        negative = negative.ToLower();

        System.Console.Clear();
        bool addAnotherRecord = AnsiConsole.Prompt(
            new TextPrompt<bool>(title)
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(false)
            .WithConverter(choice => choice ? positive : negative));     
        return addAnotherRecord;
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
        .MoreChoicesText("[Grey]Move up and down to reveal more options[/]")
        .AddChoices("Cancel")
        .AddChoices(rawOptions)
        .UseConverter(n => Regex.Replace(n, @"(?<=[A-Za-z])([A-Z])", @" $1"));

        string userOption = AnsiConsole.Prompt(selectionPrompt);

        dynamic enumCardinal;
        bool parseSuccessful = Enum.TryParse(options, userOption, out enumCardinal);

        return parseSuccessful ? enumCardinal : -1;
    }
    public static void DrawDatatable(List<CodingSession> list, bool[] viewSettings, bool automaticalDataFormatting = true)
    {

        Type type = list[0].GetType();

        Table table = new Table();


        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        string[] names = Array.ConvertAll(fields, f => f.Name);
        names = names.Skip(1).ToArray();
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = Regex.Match(names[i], @"(?<=<)[A-Za-z0-9]*(?=>)").Value;
            names[i] = Regex.Replace(names[i], @"(?<=.+)([A-Z])", @" $1");
            names[i] = names[i].Insert(0, "[yellow]");
            names[i] += "[/]";
        }

        int interval = 0;
        table.AddColumn("[yellow]Index[/]");
        foreach (string n in names)
        {
            if (viewSettings.Length > interval)
            {
                if (viewSettings[interval] == true)
                {
                    table.AddColumn(n);
                }
            }
            else
            {
                table.AddColumn(n);
            }
            interval++;
        }

        interval = 0;
        foreach (CodingSession line in list)
        {
            interval++;
            List<string> newRow = new();
            newRow.Add(interval.ToString());
            for (int i = 1; i < fields.Length; i++)
            {
                if (viewSettings[i - 1] == true)
                {
                    string rowValue = fields[i].GetValue(line) == null ? "" : fields[i].GetValue(line).ToString();
                    if (automaticalDataFormatting)
                    {
                        bool isMatch = Regex.IsMatch(rowValue, @"(?<=^1)\.+(?=\d{2}:\d{2}:\d{2}$)");
                        if (isMatch)
                        {
                            rowValue = Regex.Replace(rowValue, @"(?<=^1)\.+(?=\d{2}:\d{2}:\d{2}$)", @" day, ");
                        }
                        else
                        {
                            rowValue = Regex.Replace(rowValue, @"(?<=^[0-9]*)\.+(?=\d{2}:\d{2}:\d{2}$)", @" days, ");
                        }
                        rowValue = rowValue == "-1" ? "-" : rowValue;
                    }
                    newRow.Add(rowValue);
                }
            }
            string[] rowToAdd = newRow.ToArray();
            table.AddRow(rowToAdd);
        }
        table.Title("View previous records:", new Style().Foreground(Color.LightPink3));
        table.Expand();
        table.Border = TableBorder.DoubleEdge;
        table.ShowRowSeparators();
        table.BorderColor(Color.SteelBlue3);
        AnsiConsole.Write(table);
    }
    public static void DisplayMultiselectionUI(string title, Type options, ref bool[] updateArray)
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
                stringOptions[i] += ":" + (updateArray[i] ? "☑ " : "☐ ");
            }

            selectionPrompt
            .Title(title)
            .WrapAround(true)
            .HighlightStyle(new Style()
                .Foreground(Color.Olive)
                .Decoration(Decoration.RapidBlink))
            .EnableSearch()
            .PageSize(15)
            .MoreChoicesText("[Grey]Move up and down to reveal more options[/]")
            .UseConverter((string n) => (Regex.Replace(n, @"([A-Z][^:])", @" $1")))
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
                    title = "[red]At least one value must be selected for your table[/]";
                    updateArray[optionCardinal] = !updateArray[optionCardinal];
                }
            }
        }
    }
}
