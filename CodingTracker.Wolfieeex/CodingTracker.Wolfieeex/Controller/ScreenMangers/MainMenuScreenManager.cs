using CodingTracker.Wolfieeex.Controller.CRUD;
using CodingTracker.Wolfieeex.Controller.SQL; 
using CodingTracker.Wolfieeex.View;
using Spectre.Console;

namespace CodingTracker.Wolfieeex.Controller.ScreenMangers;

internal class MainMenuScreenManager
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

            int? userOption = UserInterface.DisplaySelectionUI("[red]Function[/] selection:", typeof(MenuSelections.MainMenu), Color.IndianRed1_1);
            switch (userOption)
            {
                case 0:
                    CrudController.AddNewSessionManually();
                    break;
                case 1:
                    CrudController.TrackNewSession();
                    break;
                case 2:
                    GoalSettings.GoalMenu();
                    break;
                case 3:
                    CrudController.ViewPreviousSessions();
                    break;
                case 4:
                    CRUD.Reporting.GenerateReport();
                    break;
                case 5:
                    CrudController.UpdateSessionDetails();
                    break;
                case 6:
                    CrudController.DeleteSession();
                    break;
                case 7:
                    System.Console.Clear();
                    AnsiConsole.Write(new Markup("You have exited the app and you will return to the main desktop\n[lightGreen]See you soon![/]").Centered());
                    menuLoop = false;
                    break;
            }
        }
    }
}
