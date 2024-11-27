using Console.CodingTracker.Model;
using Console.CodingTracker.View;
using Microsoft.VisualBasic.FileIO;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.Controller;

internal class CRUDController
{
    internal static void TrackNewSession()
    {
        string? start = null;
        string? end = null;
        int? lines = null;
        string? comments = null;

        bool trackNewSessionLoop = true;
        while (trackNewSessionLoop)
        {
            System.Console.Clear();

            Dictionary<string, string> dic = new()
            {
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)0), null},
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)1), start },
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)2), end },
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)3), lines == null ? null : lines.ToString() },
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)4), comments }
            };

            int? userOption = UserInterface.DisplaySelectionUIWithUserInputs("Track your [violet]new session[/]", typeof(MenuSelections.TrackNewSession), Color.DodgerBlue1, dic);

            switch (userOption)
            {
                case 0:
                    throw new NotImplementedException();
                    break;
                case 1:
                    start = UserInterface.DisplayTextUI("Please insert [DarkBlue]the start of the session[/]", TextUIOptions.Any);
                    break;
                case 2:
                    end = UserInterface.DisplayTextUI("Please insert [DarkBlue]the end of the session[/]", TextUIOptions.Any);
                    break;
                case 3:
                    string l = UserInterface.DisplayTextUI("Please insert [DarkBlue]number of lines you produced[/] during your session", TextUIOptions.NumbersOnlyOptional);
                    if (l != null && l != "")
                    {
                        l.Trim();
                        lines = int.Parse(l);
                    }
                    break;
                case 4:
                    comments = UserInterface.DisplayTextUI("Please insert [DarkBlue]any comments[/] you want to add", TextUIOptions.Optional);
                    break;
            }
                   
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
