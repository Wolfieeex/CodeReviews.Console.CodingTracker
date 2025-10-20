using CodingTracker.Wolfieeex.Controller;
using Spectre.Console;
using CodingTracker.Wolfieeex.View;

try
{
	var dataAccess = new DataInitializer();

	DataInitializer dataInitializer = new DataInitializer();
	bool mainDbWasInitiated = dataInitializer.InstantiateMainDatabase();
	bool goalDbWasInitiated = dataInitializer.InstantiateGoalDatabase();
	dataInitializer.SetDefaultSettings(mainDbWasInitiated, goalDbWasInitiated);

	MainMenu mainMenu = new MainMenu(Color.Aqua);
	mainMenu.DisplayMenu();
}
catch (Exception ex)
{
	Console.Clear();
	Console.WriteLine(ex.Message);
	Console.WriteLine(ex.StackTrace);
}

