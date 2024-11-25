using Console.CodingTracker.Controller;

try
{
    ScreenController.S_Mainmenu();
}
catch (Exception ex)
{
    System.Console.Clear();
    System.Console.WriteLine(ex.Message);
}