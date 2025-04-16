using Console.CodingTracker.Controller.CRUD;
using Console.CodingTracker.MenuSelections;
using Console.CodingTracker.Controller.SQL; 
using Console.CodingTracker.View;
using Spectre.Console;
using System.Configuration;

namespace Console.CodingTracker.Controller.ScreenMangers;

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
                    CRUDController.AddNewSessionManually();
                    break;
                case 1:
                    CRUDController.TrackNewSession();
                    break;
                case 2:
                    GoalSettings.GoalMenu();
                    break;
                case 3:
                    CRUDController.ViewPreviousSessions();
                    break;
                case 4:
                    CRUD.Reporting.GenerateReport();
                    break;
                case 5:
                    CRUDController.UpdateSessionDetails();
                    break;
                case 6:
                    CRUDController.DeleteSession();
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
