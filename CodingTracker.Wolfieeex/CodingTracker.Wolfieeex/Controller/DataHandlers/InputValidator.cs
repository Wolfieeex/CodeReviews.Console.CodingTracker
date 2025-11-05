using Spectre.Console;
using System.Text.RegularExpressions;

namespace CodingTracker.Wolfieeex.Controller;

public enum ValidatorType
{
    Text,
    Number,
    Datetime
}

internal static class InputValidator
{
    internal static string? ValidateInput(string title, ValidatorType validator = ValidatorType.Text)
    {
        DisplayExitInputs();

        string? input = AnsiConsole.Prompt(new TextPrompt<string?>(title)
        .AllowEmpty()
        .Validate((s) => s.ToLower() switch
        {
            string when Regex.IsMatch(s, @"[^a-zA-Z0-9\-\.;,]") => ValidationResult.Error(
                "Your text contains special characters that cannot be used in your input. " +
                "You can only use commas, semicolons, hyphens, white spaces and full stops."),
            _ => ValidationResult.Success()
        }
        ));

        if (String.IsNullOrEmpty(input))
            return null;
        else if (input.ToLower() == "e")
            return "";
        else
            return input;
    }

    internal static void ValidateIntInput(ref TextPrompt<string> prompt, ValidatorType validator)
    {
        
    }

    private static void DisplayExitInputs()
    {
        AnsiConsole.Write(new Markup("Press Enter without any input to erase the previous selection." +
                        "\nPress \"e\" to return to menu without changing the input.",
                        style: new Style(decoration: Decoration.RapidBlink)).Justify(Justify.Right));

        Console.SetCursorPosition(0, 1);
    }
}