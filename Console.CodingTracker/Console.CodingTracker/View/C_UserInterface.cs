using Spectre.Console;

namespace Console.CodingTracker.View;

internal abstract class UserInterface
{
    public abstract int? DisplayUI(string title, Type options, Color highlightcolor);   
}
