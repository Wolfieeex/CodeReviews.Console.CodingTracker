using CodingTracker.Wolfieeex.Controller;
using CodingTracker.Wolfieeex.Model;
using Console.CodingTracker.Controller;
using Console.CodingTracker.Controller.ScreenMangers;

try
{
	var dataAccess = new DataInitializer();

	// Still working on that lol
	TemporaryData.SetFilterSettingsToDefaultSettings();

	(bool mainDb, bool goalDb) = ProgramSetup.InstantiateDatabase();
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

