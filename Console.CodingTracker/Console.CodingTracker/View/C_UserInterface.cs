using Spectre.Console;

namespace Console.CodingTracker.View;

internal abstract class C_UserInterface
{
    public abstract int? DisplayUI(string title, Type options, Color highlightcolor);   
}
