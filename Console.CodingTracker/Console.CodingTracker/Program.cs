using Console.CodingTracker;
using Console.CodingTracker.MenuSelections;
using Spectre.Console;

System.Console.Clear();
AnsiConsole.Write(
    new FigletText("Coding Tracker")
        .Centered()
        .Color(Color.Red));

UserInterfaceSelection UI = new UserInterfaceSelection();
UI.DisplayUI("Please select one of the [red]functions[/] below", typeof(MainMenu));
System.Console.WriteLine(Enum.GetName(typeof(MainMenu), 0));