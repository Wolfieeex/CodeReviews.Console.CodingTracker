using CodingTracker.Wolfieeex.Controller;
using CodingTracker.Wolfieeex.Model;
using CodingTracker.Wolfieeex.Controller.ScreenMangers;

try
{
	var dataAccess = new DataInitializer();

	// Done
	TemporaryData.SetFilterSettingsToDefaultSettings();

	// In Progress
	DataInitializer dataInitializer = new DataInitializer();
	bool mainDbWasInitiated = dataInitializer.InstantiateMainDatabase();
	bool goalDbWasInitiated = dataInitializer.InstantiateGoalDatabase();
	
	// Untouched
	ProgramSetup.DisplayDevOptionSetting(mainDb, goalDb);
	ProgramSetup.ConsoleSettings();
	MainMenuScreenManager.S_Mainmenu();
}
catch (Exception ex)
{
	System.Console.Clear();
	System.Console.WriteLine(ex.Message);
	System.Console.WriteLine(ex.StackTrace);
}

