using CodingTracker.Wolfieeex.Controller;
using CodingTracker.Wolfieeex.Controller.ScreenMangers;

try
{
	var dataAccess = new DataInitializer();

	DataInitializer dataInitializer = new DataInitializer();
	bool mainDbWasInitiated = dataInitializer.InstantiateMainDatabase();
	bool goalDbWasInitiated = dataInitializer.InstantiateGoalDatabase();
	dataInitializer.SetDefaultSettings(mainDbWasInitiated, goalDbWasInitiated);
	
	MainMenuScreenManager.S_Mainmenu();
}
catch (Exception ex)
{
	Console.Clear();
	Console.WriteLine(ex.Message);
	Console.WriteLine(ex.StackTrace);
}

