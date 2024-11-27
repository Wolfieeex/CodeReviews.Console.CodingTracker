using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;

namespace Console.CodingTracker.Controller;

internal class ScreenController
{
    internal static void S_Mainmenu()
    {
        bool menuLoop = true;
        while (menuLoop)
        {
            System.Console.Clear();
            AnsiConsole.Write(
                new FigletText("Coding Tracker")
                    .Centered()
                    .Color(Color.Red));

            int? userOption = UserInterface.DisplaySelectionUI("[red]Functtion[/] selection:", typeof(MenuSelections.MainMenu), Color.IndianRed1_1);
            switch (userOption)
            {
                case 0:
                    CRUDController.TrackNewSession();
                    break;
                case 1:
                    GoalSetter.SetNewGoals();
                    break;
                case 2:
                    CRUDController.ViewPreviousSessions();
                    break;
                case 3:
                    CRUDController.UpdateSessionDetails();
                    break;
                case 4:
                    CRUDController.DeleteSession();
                    break;
                case 5:
                    System.Console.Clear();
                    AnsiConsole.Write(new Markup("You have exited the app and you will return to the main desktop\n[lightGreen]See you soon![/]").Centered());
                    menuLoop = false;
                    break;
            }
        }
    }
    internal static void S_FilterMenu()
    {
        throw new NotImplementedException();
    }
}
