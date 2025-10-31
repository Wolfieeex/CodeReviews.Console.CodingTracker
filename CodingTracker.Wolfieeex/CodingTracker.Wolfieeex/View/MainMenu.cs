using CodingTracker.Wolfieeex.Controller.Crud;
using CodingTracker.Wolfieeex.Controller.SQL;
using CodingTracker.Wolfieeex.Model;
using Spectre.Console;

namespace CodingTracker.Wolfieeex.View;

internal class MainMenu : Menu
{
    protected override string title => "Welcome to the Coding Tracker! Please select your option from the main menu: ";
    protected override Type selectionEnum => typeof(MainMenuSelections);
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
                // I'm here now! :D
                case MainMenuSelections.AddRecordManually:
                    AddRecordMenuallyMenu addRecordMenuallyMenu = new AddRecordMenuallyMenu(Color.Plum1);
                    addRecordMenuallyMenu.DisplayMenu();
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
