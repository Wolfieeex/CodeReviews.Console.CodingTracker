using Console.CodingTracker.Model;
using Console.CodingTracker.View;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.Controller;

internal class CRUDController
{
    internal static void TrackNewSession()
    {
        string start;
        string end;
        int? lines;
        string comments;

        bool trackNewSessionLoop = true;
        while (trackNewSessionLoop)
        {
            System.Console.Clear();

            UserInterfaceSelection UI = new();
            UI.DisplayUI("Track your [violet]new session[/]", typeof(MenuSelections.TrackNewSession), Color.DodgerBlue1);
            
            UserInterfaceText UIT = new();
            System.Console.WriteLine(" d");
            System.Console.ReadKey();
            int? userOptionT = UIT.DisplayUI("[red]Functtion[/] selection:", typeof(MenuSelections.MainMenu), Color.IndianRed1_1);
       
        }
    }

    internal static void ViewPreviousSessions()
    {
        throw new NotImplementedException(); 
    }

    internal static void UpdateSessionDetails()
    {
        throw new NotImplementedException();
    }

    internal static void DeleteSession()
    {
        throw new NotImplementedException();
    }
}
