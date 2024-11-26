using Spectre.Console;

namespace Console.CodingTracker.View;

internal class UserInterfaceText : C_UserInterface
{
    public override int? DisplayUI(string title, Type options, Color highlightColor)
    {
        AnsiConsole.Prompt(
            new TextPrompt<string>(title)
            .Validate(i => i switch 
            {
                "mama" => ValidationResult.Error("Too low"),
                "ciocia" => ValidationResult.Error("Too high"),
                _ => ValidationResult.Success(),
            })
        );

        return 0;
    }
}