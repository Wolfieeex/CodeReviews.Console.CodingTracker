using Console.CodingTracker.Model;
using Console.CodingTracker.View;
using Microsoft.VisualBasic.FileIO;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.Controller;

internal class CRUDController
{
    internal static void AddNewSessionManually()
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
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)0), start },
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)1), end },
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)2), lines == null ? null : lines.ToString() },
                { Enum.GetName(typeof(MenuSelections.TrackNewSession), (MenuSelections.TrackNewSession)3), comments }
            };

            int? userOption = null;
            try
            {
                userOption = UserInterface.DisplaySelectionUIWithUserInputs("Track your [violet]new session:[/]", typeof(MenuSelections.TrackNewSession), Color.DodgerBlue1, dic, "[green]AddRecord[\\]");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(@"DisplaySelectionUIWithUserInputs method failed: " + ex.Message);
                System.Console.ReadKey();
            }
            finally
            {
                switch (userOption)
                {
                    case 0:
                        start = UserInterface.DisplayTextUI("Please insert [Blue]the start of the session[/] in \"dd/mm/yyyy, hh:mm\" format: ", TextUIOptions.DateOnly);
                        start.Trim();
                        break;
                    case 1:
                        end = UserInterface.DisplayTextUI("Please insert [Blue]the end of the session[/] in \"dd/mm/yyyy, hh:mm\" format: ", TextUIOptions.DateOnly);
                        end.Trim();
                        break;
                    case 2:
                        string l = UserInterface.DisplayTextUI("Please insert [Blue]number of lines you produced[/] during your session: ", TextUIOptions.NumbersOnlyOptional);
                        if (l != null && l != "")
                        {
                            l.Trim();
                            lines = int.Parse(l);
                        }
                        break;
                    case 3:
                        comments = UserInterface.DisplayTextUI("Please insert [Blue]any comments[/] you want to add: ", TextUIOptions.Optional);
                        comments = string.IsNullOrEmpty(comments) ? comments : comments.Trim();
                        break;
                    case 4:
                        trackNewSessionLoop = false;
                        break;
                    case -1:
                        SQLCommands.InjectRecord();
                        break;
                }
            }
        }
    }
    internal static void TrackNewSession()
    {
        throw new NotImplementedException();
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
