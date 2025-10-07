using CodingTracker.Wolfieeex.Controller.Crud;
using CodingTracker.Wolfieeex.Controller.SQL;
using CodingTracker.Wolfieeex.MenuSelections;
using Spectre.Console;

namespace CodingTracker.Wolfieeex.View;

internal class MainMenu : Menu
{
    public MainMenu(Color color) : base(color) {}
    public override void DisplayMenu()
    {
        bool menuLoop = true;
        while (menuLoop)
        {
            Console.Clear();
            AnsiConsole.Write(
                new FigletText("Coding Tracker")
                    .Centered()
                    .Color(Color.Red));

            var userOption = DisplayOptions();

            switch (userOption)
            {
                case MainMenuSelections.AddRecordManually:
                    CrudController.AddNewSessionManually();
                    break;
                case MainMenuSelections.StartTrackingNewSession:
                    CrudController.TrackNewSession();
                    break;
                case MainMenuSelections.SetGoals:
                    GoalSettings.GoalMenu();
                    break;
                case MainMenuSelections.ViewPreviousSessions:
                    CrudController.ViewPreviousSessions();
                    break;
                case MainMenuSelections.GenerateReport:
                    Controller.Crud.Reporting.GenerateReport();
                    break;
                case MainMenuSelections.UpdateSessionDetails:
                    CrudController.UpdateSessionDetails();
                    break;
                case MainMenuSelections.DeleteSession:
                    CrudController.DeleteSession();
                    break;
                case MainMenuSelections.ExitApp:
                    Console.Clear();
                    AnsiConsole.Write(new Markup("You have exited the app and you will return to the main desktop\n[lightGreen]See you soon![/]").Centered());
                    menuLoop = false;
                    break;
            }
        }
    }
}
