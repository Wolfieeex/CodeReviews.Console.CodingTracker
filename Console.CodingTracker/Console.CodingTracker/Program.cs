using Console.CodingTracker.Controller;
using Console.CodingTracker.Model;

try
{
    ProgramSetup.InstantiateDatabase();
    ScreenController.S_Mainmenu();
}
catch (Exception ex)
{
    System.Console.Clear();
    System.Console.WriteLine(ex.Message);
}