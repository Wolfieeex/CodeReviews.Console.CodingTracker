using Console.CodingTracker.Controller;
using Console.CodingTracker.Model;

try
{
    TemporaryData.InitializeLastFilter();
    ProgramSetup.InstantiateDatabase();
    ScreenController.S_Mainmenu();
}
catch (Exception ex)
{
    System.Console.Clear();
    System.Console.WriteLine(ex.Message);
    System.Console.WriteLine(ex.StackTrace);
}