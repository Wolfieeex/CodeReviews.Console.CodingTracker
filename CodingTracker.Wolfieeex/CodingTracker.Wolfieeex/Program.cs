using CodingTracker.Wolfieeex.Controller;
using CodingTracker.Wolfieeex.Model;
using CodingTracker.Wolfieeex.Controller.ScreenMangers;

try
{
	var dataAccess = new DataInitializer();

	// Filter should automatically, upon constructing, set itself to its default value!!!
	TemporaryData.SetFilterSettingsToDefaultSettings();
	DataInitializer dataInitializer = new DataInitializer();
	bool mainDbWasInitiated = dataInitializer.InstantiateMainDatabase();
	bool goalDbWasInitiated = dataInitializer.InstantiateGoalDatabase();
	dataInitializer.SetDefaultSettings(mainDbWasInitiated, goalDbWasInitiated);
	
	// In Progress
	MainMenuScreenManager.S_Mainmenu();
}
catch (Exception ex)
{
	Console.Clear();
	Console.WriteLine(ex.Message);
	Console.WriteLine(ex.StackTrace);
}

