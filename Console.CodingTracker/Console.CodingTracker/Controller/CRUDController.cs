﻿using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;
using System.Timers;
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
                string temp = "";
                switch (userOption)
                {
                    case 0:
                        temp = UserInterface.DisplayTextUI("Please insert [Blue]the start of the session[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnly);
                        if (temp.ToLower() == "e")
                        {
                            break;
                        }
                        start = temp;
                        start.Trim();
                        break;
                    case 1:
                        temp = UserInterface.DisplayTextUI("Please insert [Blue]the end of the session[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnly);
                        if (temp.ToLower() == "e")
                        {
                            break;
                        }
                        end = temp;
                        end.Trim();
                        break;
                    case 2:
                        temp = UserInterface.DisplayTextUI("Please insert [Blue]number of lines you produced[/] during your session. ", TextUIOptions.NumbersOnlyOptional);
                        if (temp.ToLower() == "e")
                        {
                            break;
                        }
                        if (temp != null && temp != "")
                        {
                            temp.Trim();
                            lines = int.Parse(temp);
                        }
                        else
                        {
                            lines = null;
                        }
                        break;
                    case 3:
                        temp = UserInterface.DisplayTextUI("Please insert [Blue]any comments[/] you want to add. ", TextUIOptions.Optional);
                        if (temp.ToLower() == "e")
                        {
                            break;
                        }
                        temp = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                        comments = temp;
                        break;
                    case 4:
                        trackNewSessionLoop = false;
                        break;
                    case -1:
                        string duration = SQLCommands.InjectRecord(new Session(DateTime.Now.ToString($"dd/MM/yyyy, HH:mm"), 
                                                                               DateTime.Now.ToString($"dd/MM/yyyy, HH:mm"), 
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
        System.Console.Clear();
        bool trackNewSessionLoop = true;
        while (trackNewSessionLoop)
        {
            int? option = UserInterface.DisplaySelectionUI("Start tracking your [yellow2]new session[/]:", typeof(MenuSelections.RecordSessionStartMenu), Color.GreenYellow);

            if (option == 1)
            {
                return;
            }
            System.Console.Clear();
            int secondsPassed = 0;
            DisplayTimer(secondsPassed);

            DateTime sessionStart = DateTime.Now;

            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += TimerEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            bool trackerOn = true;
            while (trackerOn)
            {
                bool sessionDiscarted = false;
                option = UserInterface.DisplaySelectionUI(timer.Enabled ? "[blue]Your session is in progress:[/]" : "[blue]Your session is[/] [red]paused:[/]", timer.Enabled ? typeof(MenuSelections.RecordSessionRecording) : typeof(MenuSelections.RecordSessionPause), Color.LightSteelBlue);

                switch (option)
                {
                    case 0:
                        timer.Enabled = !timer.Enabled;
                        break;
                    case 1:
                        timer.Stop();
                        if (UserInterface.DisplayConfirmationSelection("Are you sure you want to [red]discard this session?[/]", "yes", "no"))
                        {
                            timer.Close();
                            trackerOn = false;
                            System.Console.Clear();
                            break;
                        }
                        System.Console.Clear();
                        DisplayTimer(secondsPassed);
                        timer.Start();
                        break;
                    case 2:
                        timer.Stop();
                        System.Console.Clear();
                        if (UserInterface.DisplayConfirmationSelection("Are you sure you want to [yellow]end this session?[/]", "yes", "no"))
                        {
                            

                            System.Console.Clear();
                            string input = AnsiConsole.Prompt(
                                new TextPrompt<string>("Please [blue]insert the number of lines produced[/]. If you changed your mind and want to [red]discard this timer[/], insert [red]\"D\"[/]. If you want to [green]continue tracking[/], insert [green]\"R\"[/]: ")
                                .Validate((s) => s.ToLower() switch
                                {
                                    ("r") => ValidationResult.Success(),
                                    ("d") => ValidationResult.Success(),
                                    string when Int32.TryParse(s, out _) && Int32.Parse(s) > 0 => ValidationResult.Success(),
                                    _ => ValidationResult.Error("Please [green]enter \"R\" to resume[/], [red]\"D\" to discard[/], or [blue]valid number to continue[/]: ")
                                })
                                );
                            switch (input.ToLower())
                            {
                                case "r":
                                    break;
                                case "d":
                                    System.Console.Clear();
                                    if (UserInterface.DisplayConfirmationSelection("Are you sure you want to [red]discard this session?[/]", "yes", "no"))
                                    {
                                        sessionDiscarted = true;
                                        timer.Stop();
                                        timer.Close();
                                        trackerOn = false;
                                        System.Console.Clear();
                                    }
                                    break;
                                default:
                                    int numberOfLines = int.Parse(input);

                                    System.Console.Clear();
                                    input = AnsiConsole.Prompt(
                                    new TextPrompt<string>("Please [blue]insert any comments you want to add[/]. If you changed your mind and want to [red]discard this timer[/], insert [red]\"D\"[/]. If you want to [green]continue tracking[/], insert [green]\"R\"[/]: ")
                                    .Validate((s) => s.ToLower() switch
                                    {
                                        ("r") => ValidationResult.Success(),
                                        ("d") => ValidationResult.Success(),
                                        string => ValidationResult.Success(),
                                        _ => ValidationResult.Error("Please enter [green]\"R\" to resume[/], [red]\"D\" to discard[/], or a [blue]comment to continue[/]: ")
                                    })
                                    );

                                    switch (input.ToLower())
                                    {
                                        case "r":
                                            break;
                                        case "d":
                                            System.Console.Clear();
                                            if (UserInterface.DisplayConfirmationSelection("Are you sure you want to [red]discard this session?[/]", "yes", "no"))
                                            {
                                                sessionDiscarted = true;
                                                timer.Close();
                                                trackerOn = false;
                                                System.Console.Clear();
                                                break;
                                            }
                                            break;
                                        default:
                                            SQLCommands.InjectRecord(new Session((sessionStart + TimeSpan.FromSeconds(secondsPassed)).ToString($"dd/MM/yyyy, HH:mm"),
                                                                                (sessionStart + TimeSpan.FromSeconds(secondsPassed)).ToString($"dd/MM/yyyy, HH:mm"),
                                                                                sessionStart.ToString($"dd/MM/yyyy, HH:mm"),
                                                                                (sessionStart + TimeSpan.FromSeconds(secondsPassed)).ToString($"dd/MM/yyyy, HH:mm"),
                                                                                TimeSpan.FromSeconds(secondsPassed).ToString(),
                                                                                numberOfLines,
                                                                                input,
                                                                                true));
                                            System.Console.Clear();
                                            if (!UserInterface.DisplayConfirmationSelection($"Coding session of duration [Pink1]{TimeSpan.FromSeconds(secondsPassed).ToString()} has been added![/]\nWould you like to [Pink1]start another session[/], or [Pink1]return to the main menu[/]?:\n", "Start", "Return"))
                                            {
                                                sessionDiscarted = true;
                                                timer.Close();
                                                trackerOn = false;
                                                System.Console.Clear();
                                                return;
                                            }
                                            secondsPassed = 0; 
                                            System.Console.Clear();
                                            break;

                                    }
                                    break;
                            }
                        }
                        if (!sessionDiscarted) 
                        {
                            System.Console.Clear();
                            DisplayTimer(secondsPassed);
                            timer.Start();
                        }
                        break;
                }
            }

            void TimerEvent(Object source, ElapsedEventArgs e)
            {
                secondsPassed++;
                DisplayTimer(secondsPassed);
            }
        }

        void DisplayTimer(int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            bool isMatch = Regex.IsMatch(timeSpan.ToString(), @"(?<=^1)\.");

            System.Console.Clear();
            AnsiConsole.Write(
            new FigletText(Regex.Replace(timeSpan.ToString(), @"\.", isMatch ? " day, " : " days, "))
                .Centered()
                .Color(Color.SteelBlue1_1));
        }
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
                AnsiConsole.Write(new Markup("\n[green]Press any key[/] to return to previous menu:"));
            }
            else if (Filter == null)
            {
                AnsiConsole.Write(new Markup("[red]No records found[/]. [green]Press any key[/] to return to previous menu:"));
            }
            else
            {
                AnsiConsole.Write(new Markup("[red]No records found with selected filters[/]. [green]Press any key[/] to return to previous menu:"));
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

            string temp = "";
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
                    if (UserInterface.DisplayConfirmationSelection("Are you sure you want to remove all your previous filters?", "Yes", "No"))
                    {
                        start = null;
                        end = null;
                        linesMin = null;
                        linesMax = null;
                        comments = null;
                        durationMin = null;
                        durationMax = null;
                    }
                    break;
                case 1:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the date from which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    start = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 2:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the date to which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    end = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 3:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the minimal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    linesMin = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 4:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the maximal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    linesMax = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 5:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]part of the comment[/] you want to search for. ", TextUIOptions.Optional);
                    if (temp == "e")
                    {
                        break;
                    }
                    comments = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 6:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]minimal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    durationMin = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 7:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]maximal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    durationMax = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 8:
                    returnToMenu = true;
                    runFilterMenuLoop = false;
                break;
            }
        }
        return null;
    }

    internal static void GenerateReport()
    {
        throw new NotImplementedException();
    }
}
