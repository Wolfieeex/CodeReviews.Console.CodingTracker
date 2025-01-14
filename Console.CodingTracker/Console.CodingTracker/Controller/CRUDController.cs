using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;
using Microsoft.Data.Sqlite;
using SQLitePCL;

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

            bool blockEndOption = false;
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                DateTime startInDate = DateTime.Parse(start);
                DateTime endInDate = DateTime.Parse(end);
                if (startInDate > endInDate)
                {
                    blockEndOption = true;
                }
            }

            int? userOption = null;
            try
            {
                userOption = UserInterface.DisplaySelectionUIWithUserInputs("Track your [violet]new session:[/]", typeof(MenuSelections.TrackNewSession), Color.DodgerBlue1, dic, "[green]AddRecord[/]", blockEndOption, "[red]The start date of your session must be before the end date of your session:[/]");
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
                        string duration = SQLCommands.InjectRecord(new Session(DateTime.Now.ToString($"dd/MM/yyyy, hh:mm"), 
                                                                               DateTime.Now.ToString($"dd/MM/yyyy, hh:mm"), 
                                                                               start,
                                                                               end,
                                                                               SQLCommands.CalculateDuration(start, end).ToString(),
                                                                               lines,
                                                                               comments,
                                                                               false));
                        duration = duration.Replace(".", " days, ");
                        duration += " hours";
                        bool addAnotherRecord = UserInterface.DisplayConfirmationSelection($"Coding session of duration [Pink1]{duration} has been added![/]\nWould you like to [Pink1]add another record[/], or [Pink1]return to the main menu[/]?:\n", "Add", "Return");

                        start = null;
                        end = null;
                        lines = null;
                        comments = null;

                        if (!addAnotherRecord)
                            trackNewSessionLoop = false;

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
        FilterDetails Filter = null;
        bool returnToMenu = false;

        while (!returnToMenu)
        {
            try
            {
                Filter = FilterRecords(ref returnToMenu);
                if (returnToMenu)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.ReadKey();
            }

            List<Session> sessions = SQLCommands.GetRecords(Filter);
            if (sessions != null && sessions.Any())
            {
                bool[] tableFieldSettings = { false, false, true, true, true, true, true, false };
                UserInterface.DrawDatatable(sessions, tableFieldSettings);
                AnsiConsole.Write(new Markup("\n[green]Press any key[/] to return to the main menu:"));
            }
            else if (Filter == null)
            {
                AnsiConsole.Write(new Markup("[red]No records found[/]. [green]Press any key[/] to return to the main menu:"));
            }
            else
            {
                AnsiConsole.Write(new Markup("[red]No records found with selected filters[/]. [green]Press any key[/] to return to the main menu:"));
            }
            System.Console.ReadKey();
        }
    }

    internal static void UpdateSessionDetails()
    {
        throw new NotImplementedException();
    }

    internal static void DeleteSession()
    {
        throw new NotImplementedException();
    }

    internal static FilterDetails FilterRecords(ref bool returnToMenu)
    {
        string start = TemporaryData.lastFilter.FromDate;
        string end = TemporaryData.lastFilter.ToDate;
        string linesMin = TemporaryData.lastFilter.MinLines;
        string linesMax = TemporaryData.lastFilter.MaxLines;
        string comments = TemporaryData.lastFilter.Comment;
        string durationMin = TemporaryData.lastFilter.MinDuration;
        string durationMax = TemporaryData.lastFilter.MaxDuration;
        

        bool runFilterMenuLoop = true;
        while (runFilterMenuLoop)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)1), start},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)2), end},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)3), linesMin},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)4), linesMax},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)5), comments},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)6), durationMin},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)7), durationMax}
            };

            System.Console.Clear();

            bool shouldBlock = false;
            if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end))
            {
                DateTime dateStart = DateTime.Parse(start);
                DateTime dateEnd = DateTime.Parse(end);

                if (dateEnd < dateStart)
                {
                    shouldBlock = true;
                }
            }    

            int? userOption = UserInterface.DisplaySelectionUIWithUserInputs("Select [purple]filters[/] for your search:", typeof(MenuSelections.FilterRecords), Color.Plum2, dic, "[green]SearchRecords[/]", shouldBlock, "[red]The start date of your session must be before the end date of your session:[/]");

            switch (userOption)
            {
                case -1:
                    FilterDetails finalFilter = new FilterDetails()
                    {
                        FromDate = start,
                        ToDate = end,
                        MinLines = linesMin,
                        MaxLines = linesMax,
                        Comment = comments,
                        MinDuration = durationMin,
                        MaxDuration = durationMax
                    };
                    TemporaryData.lastFilter = finalFilter;
                    return finalFilter;
                case 0:
                    start = null;
                    end = null;
                    linesMin = null;
                    linesMax = null;
                    comments = null;
                    durationMin = null;
                    durationMax = null;
                    break;
                case 1:
                    start = UserInterface.DisplayTextUI("Please insert [Blue]the date from which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format: ", TextUIOptions.DateOnlyOptional);
                    start = string.IsNullOrEmpty(start) ? start : start.Trim();
                break;
                case 2:
                    end = UserInterface.DisplayTextUI("Please insert [Blue]the date to which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format: ", TextUIOptions.DateOnlyOptional);
                    end = string.IsNullOrEmpty(end) ? end : end.Trim();
                break;
                case 3:
                    linesMin = UserInterface.DisplayTextUI("Please insert [Blue]the minimal number of lines[/] for searched sessions: ", TextUIOptions.NumbersOnlyOptional);
                    linesMin = string.IsNullOrEmpty(linesMin) ? linesMin : linesMin.Trim();
                break;
                case 4:
                    linesMax = UserInterface.DisplayTextUI("Please insert [Blue]the maximal number of lines[/] for searched sessions: ", TextUIOptions.NumbersOnlyOptional);
                    linesMax = string.IsNullOrEmpty(linesMax) ? linesMax : linesMax.Trim();
                break;
                case 5:
                    comments = UserInterface.DisplayTextUI("Please insert [Blue]part of the comment[/] you want to search for: ", TextUIOptions.Optional);
                    comments = string.IsNullOrEmpty(comments) ? comments : comments.Trim();
                break;
                case 6:
                    durationMin = UserInterface.DisplayTextUI("Please insert [Blue]minimal duration[/] of the sessions you want to search for in \"d hh:mm\" format: ", TextUIOptions.TimeSpanOnlyOptional);
                    durationMin = string.IsNullOrEmpty(durationMin) ? durationMin : durationMin.Trim();
                break;
                case 7:
                    durationMax = UserInterface.DisplayTextUI("Please insert [Blue]maximal duration[/] of the sessions you want to search for in \"d hh:mm\" format: ", TextUIOptions.TimeSpanOnlyOptional);
                    durationMax = string.IsNullOrEmpty(durationMax) ? durationMax : durationMax.Trim();
                break;
                case 8:
                    returnToMenu = true;
                    runFilterMenuLoop = false;
                break;
            }
        }
        return null;
    }
}
