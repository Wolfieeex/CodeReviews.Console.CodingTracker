using CodingTracker.Wolfieeex.View;
using Spectre.Console;
using System.Globalization;
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
    internal static string? ValidateInput(string title, ValidatorType validator = ValidatorType.Text, MenuColors colors)
    {
        Console.Clear();
        DisplayExitInputs();

        TextPrompt<string?> prompt = new TextPrompt<string?>(title);
        AssignValidation(ref prompt, validator, colors);
        string? input = AnsiConsole.Prompt(prompt);

        Console.Clear();
        if (String.IsNullOrEmpty(input))
            return null;
        else if (input.ToLower() == "e")
            return "";
        else
            return input;
    }

    internal static void AssignValidation(ref TextPrompt<string?> prompt, ValidatorType validator, MenuColors menuColors)
    {
        switch (validator)
        {
            case ValidatorType.Text:
                prompt.Validate((s) => s.ToLower() switch
                {
                    "" => ValidationResult.Success(),
                    "e" => ValidationResult.Success(),
                    string when Regex.IsMatch(s, @"[^a-zA-Z0-9\-\.;,]") => ValidationResult.Error(
                        $"Your text contains {menuColors.titleWarningColor}special characters[/] that cannot be used in your input. " +
                        $"You can only use {menuColors.titlePositiveColor}commas, semicolons, hyphens, white spaces and full stops[/]."),
                    _ => ValidationResult.Success()
                });
                break;
            case ValidatorType.Number:
                prompt.Validate((s) => s.ToLower() switch
                {
                    "" => ValidationResult.Success(),
                    "e" => ValidationResult.Success(),
                    string when int.TryParse(s, out int f) && f < 0 => ValidationResult.Error(
                        $"The number of lines you changed must be {menuColors.titlePositiveColor}greater than 0[/]."),
                    string when int.TryParse(s, out _) => ValidationResult.Success(),
                    _ => ValidationResult.Error($"Your input must be a {menuColors.titlePositiveColor}positive integer[/].")
                });
                break;
            case ValidatorType.Datetime:
                prompt.Validate((s) => s.ToLower() switch
                {
                    "" => ValidationResult.Success(),
                    "e" => ValidationResult.Success(),
                    string when DateTime.TryParseExact(s, "dd/MM/yy HH/mm", new CultureInfo("en-GB"), DateTimeStyles.None, out _) => ValidationResult.Success(),
                    _ => ValidationResult.Error($"Your input must be a {menuColors.titlePositiveColor}date in format DD/MM/YY (day/month/year)[/] " +
                    $"separated by space {menuColors.titleWarningColor}HH/SS (hours/seconds)[/] => {menuColors.titleHighlightColor}DD/MM/YY HH/SS[/]")
                });
                break;
        }
    }

    private static void DisplayExitInputs()
    {
        AnsiConsole.Write(new Markup("Press Enter without any input to erase the previous selection." +
                        "\nPress \"e\" to return to menu without changing the input.",
                        style: new Style(decoration: Decoration.RapidBlink)).Justify(Justify.Right));

        Console.SetCursorPosition(0, 2);
    }
}