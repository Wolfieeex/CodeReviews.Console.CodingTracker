using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;
using System.Timers;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml.Linq;

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
                            lines = lines < 1 ? 1 : lines;
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
                        bool addAnotherRecord = UserInterface.DisplayConfirmationSelectionUI($"Coding session of duration [Pink1]{duration} has been added![/]\nWould you like to [Pink1]add another record[/], or [Pink1]return to the main menu[/]?:\n", "Add", "Return");

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
                        if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to [red]discard this session?[/]", "yes", "no"))
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
                        if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to [yellow]end this session?[/]", "yes", "no"))
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
                                    if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to [red]discard this session?[/]", "yes", "no"))
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
                                            if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to [red]discard this session?[/]", "yes", "no"))
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
                                            if (!UserInterface.DisplayConfirmationSelectionUI($"Coding session of duration [Pink1]{TimeSpan.FromSeconds(secondsPassed).ToString()} has been added![/]\nWould you like to [Pink1]start another session[/], or [Pink1]return to the main menu[/]?:\n", "Start", "Return"))
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
                Filter = FilterRecords("You are currently using [green]view previous sessions method[/]. ", ref returnToMenu);
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
                UserInterface.DrawDatatable(sessions, Filter.ViewOptions);
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
        bool updateMenuRun = true;
        while (updateMenuRun)
        {
            System.Console.Clear();

            bool quitMenu = false;
            FilterDetails filters = FilterRecords("You are currently using [green]update record method[/]. ", ref quitMenu);
            if (quitMenu) 
            { 
                break; 
            }
            List<Session> sessions = SQLCommands.GetRecords(filters);

            if (sessions == null || !sessions.Any())
            {
                System.Console.Clear();
                AnsiConsole.Markup("No records to update with given filters. Press any [blue]key to go back to filter menu[/]: ");
                System.Console.ReadKey();
                continue;
            }

            bool[] viewOptions = filters.ViewOptions;
            UserInterface.DrawDatatable(sessions, viewOptions);
            System.Console.WriteLine();
            int reason = 0;

            // This to be transferred into UserInterface class- this is not a controller, this belongs to View Model
            string userInput = AnsiConsole.Prompt(
                new TextPrompt<string>("\nPlese [blue]select record(s) you would like to update by choosing their index number(s)[/]. Please separate [yellow]multiple records[/] by adding \",\" between them, e.g. [green]1[/]  or  [green]23,58[/]  or  [green]8, 34, 8[/]. You can also insert [red]\"E\" to return to previous menu:[/] ")
                .Validate((s) => s.ToLower() switch
                {
                    ("e") => ValidationResult.Success(),
                    string when !IndexCheck(s, sessions.Count, ref reason) => ValidationResult.Error($"\n{(reason == 0 ? "You can only [blue]input index numbers you want to update separated by commas[/] or [red]\"E\" to return to a previous menu[/]." : "You can only select index " + (sessions.Count == 1 ? "number 1" : " numbers from 1 to " + sessions.Count))}\n"),
                    (_) => ValidationResult.Success()
                })
            );

            if (userInput.ToLower() == "e")
            {
                continue;                
            }

            int[] indexNumbers = userInput.Split(',')
                               .Select(x =>  int.Parse(x)).ToArray();
            List<Session> selectedSessions = new List<Session>();
            foreach (int i in indexNumbers)
            {
                if (!selectedSessions.Contains(sessions[i - 1]))
                {
                    selectedSessions.Add(sessions[i - 1]);
                }
            }

            updateMenuRun = UpdateMenu(selectedSessions);
        }

        bool IndexCheck(string index, int sessionsLength, ref int reason)  
        {
            if (!Regex.IsMatch(index, @"^(\s*[0-9]+\s*)(,\s*[0-9]+\s*)*$"))
            {
                reason = 0;
                return false;
            }

            string[] indexArray = index.Split(',');
            foreach (string s in indexArray)
            {
                if (int.Parse(s) > sessionsLength || int.Parse(s) < 0)
                {
                    reason = 1;
                    return false; 
                }
            }
            return true;
        }
    }
    internal static bool UpdateMenu(List<Session> sessions)
    {
        bool updateWhile = true;
        while (updateWhile)
        {
            System.Console.Clear();
            UserInterface.DrawDatatable(sessions, new bool[] { true, true, true, true, true, true, true, true });
            int? userOption = UserInterface.DisplaySelectionUI($"{(sessions.Count == 1 ? "" : "Multi - update([yellow]all selected records will be updated at the same time[/]). ")}[blue]Please make your selection:[/]", typeof(MenuSelections.UpdateMenu), Color.Orange4);

            string temp = "";
            switch (userOption)
            {

                case 0:
                    temp = UserInterface.DisplayTextUI("Please insert [yellow]the start of the session[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.StartDate, sessions.Select(x => x.Key).ToList());
                    if (temp.ToLower() == "e")
                    {
                        break;
                    }
                    if (String.IsNullOrEmpty(temp))
                    {
                        break;
                    }
                    SQLCommands.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp, MenuSelections.UpdateMenu.UpdateStartDate);
                    foreach (Session s in sessions)
                    {
                        s.StartDate = temp.Trim();
                        s.LastUpdateDate = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
                    }
                    List<string> durations = SQLCommands.GetDurations(sessions.Select(x => x.Key).ToList());
                    for (int i = 0; i < sessions.Count; i++)
                    {
                        sessions[i].Duration = durations[i];
                    }
                    break;

                case 1:
                    temp = UserInterface.DisplayTextUI("Please insert [yellow]the end of the session[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.EndDate, sessions.Select(x => x.Key).ToList());
                    if (temp.ToLower() == "e")
                    {
                        break;
                    }
                    if (String.IsNullOrEmpty(temp))
                    {
                        break;
                    }
                    SQLCommands.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp, MenuSelections.UpdateMenu.UpdateEndDate);
                    foreach (Session s in sessions)
                    {
                        s.EndDate = temp.Trim();
                        s.LastUpdateDate = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
                    }
                    durations = SQLCommands.GetDurations(sessions.Select(x => x.Key).ToList());
                    for (int i = 0; i < sessions.Count; i++)
                    {
                        sessions[i].Duration = durations[i];
                    }
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
                        int lines = int.Parse(temp);
                        lines = lines < 1 ? 1 : lines;
                        temp = lines.ToString();
                    }
                    else
                    {
                        temp = null;
                    }

                    SQLCommands.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp == null ? "" : temp, MenuSelections.UpdateMenu.UpdateNumberOfLines);
                    foreach (Session s in sessions)
                    {
                        s.NumberOfLines = temp == null ? null : int.Parse(temp);
                        s.LastUpdateDate = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
                    }
                    break;
                case 3:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]any comments[/] you want to add. ", TextUIOptions.Optional);
                    if (temp.ToLower() == "e")
                    {
                        break;
                    }
                    temp = string.IsNullOrEmpty(temp) ? temp : temp.Trim();

                    SQLCommands.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp == null ? "" : temp, MenuSelections.UpdateMenu.UpdateComments);
                    foreach (Session s in sessions)
                    {
                        s.Comments = temp;
                        s.LastUpdateDate = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
                    }
                    break;
                case 4:
                    return true;
                case 5:
                    return false;
            }
        }
        return false;
    }
    internal static void DeleteSession()
    {
        throw new NotImplementedException();
    }
    internal static FilterDetails FilterRecords(string preTitle, ref bool returnToMenu)
    {
        SortingDetails sortingDetails = TemporaryData.lastFilter.SortingDetails;
        bool[] viewOptions = TemporaryData.lastFilter.ViewOptions;
        string start = TemporaryData.lastFilter.FromDate;
        string end = TemporaryData.lastFilter.ToDate;
        string linesMin = TemporaryData.lastFilter.MinLines;
        string linesMax = TemporaryData.lastFilter.MaxLines;
        string comments = TemporaryData.lastFilter.Comment;
        string durationMin = TemporaryData.lastFilter.MinDuration;
        string durationMax = TemporaryData.lastFilter.MaxDuration;
        string wasTimerTracked = TemporaryData.lastFilter.WasTimerTracked;
        
        bool runFilterMenuLoop = true;
        while (runFilterMenuLoop)
        {
            string sortingDetailsString = sortingDetails.SortBy == null || sortingDetails.SortOrder == null ? null : sortingDetails.SortOrder.ToString() + ", " + Regex.Replace(sortingDetails.SortBy.ToString(), @"(?<=[A-Za-z])([A-Z])", @" $1");
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)2), sortingDetailsString},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)3), start},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)4), end},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)5), linesMin},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)6), linesMax},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)7), comments},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)8), durationMin},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)9), durationMax},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)10), wasTimerTracked}
            };

            System.Console.Clear();

            string reason = "";
            bool shouldBlock = false;
            if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end))
            {
                DateTime dateStart = DateTime.Parse(start);
                DateTime dateEnd = DateTime.Parse(end);

                if (dateEnd < dateStart)
                {
                    reason += "[red]The start date of your session must be before the end date of your session.[/]\n";
                    shouldBlock = true;
                }
            }
            if (!String.IsNullOrEmpty(linesMax) && !String.IsNullOrEmpty(linesMin))
            {

                if (Int32.Parse(linesMax) < Int32.Parse(linesMin))
                {
                    reason += "[red]Minimal number of lines cannot exceed maximal lines search.[/]\n";
                    shouldBlock = true;
                }
            }
            if (!String.IsNullOrEmpty(durationMax) && !String.IsNullOrEmpty(durationMin))
            {
                if (TimeSpan.ParseExact(durationMax, @"d\ hh\:mm", new CultureInfo("en-GB"), TimeSpanStyles.None) < TimeSpan.ParseExact(durationMin, @"d\ hh\:mm", new CultureInfo("en-GB"), TimeSpanStyles.None))
                {
                    reason += "[red]Your maximal session time needs to be longer than the minimal session time.[/]\n";
                    shouldBlock = true;
                }
            }

            int? userOption = UserInterface.DisplaySelectionUIWithUserInputs(preTitle + "Select [purple]filters[/] for your search:", typeof(MenuSelections.FilterRecords), Color.Plum2, dic, "[green]SearchRecords[/]", shouldBlock, reason);

            string temp = "";
            switch (userOption)
            {
                case -1:
                    FilterDetails finalFilter = new FilterDetails()
                    {
                        SortingDetails = sortingDetails,
                        ViewOptions = viewOptions,
                        FromDate = start,
                        ToDate = end,
                        MinLines = linesMin,
                        MaxLines = linesMax,
                        Comment = comments,
                        MinDuration = durationMin,
                        MaxDuration = durationMax,
                        WasTimerTracked = wasTimerTracked
                    };
                    TemporaryData.lastFilter = finalFilter;
                    return finalFilter;
                case 0:
                    if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to remove all your previous filters?", "Yes", "No"))
                    {
                        sortingDetails = new SortingDetails()
                        {
                            SortBy = null,
                            SortOrder = null
                        };
                        viewOptions = new bool[] { false, false, false, false, true, true, true, false };
                        start = null;
                        end = null;
                        linesMin = null;
                        linesMax = null;
                        comments = null;
                        durationMin = null;
                        durationMax = null;
                        wasTimerTracked = null;
                    }
                    break;
                case 1:
                    UserInterface.DisplayMultiselectionUI("Please [yellow4]toggle select elements you want your datatable to include[/]:", typeof(MenuSelections.TableViewMenu), ref viewOptions);
                    break;
                case 2:
                    sortingDetails = SortingMenu(sortingDetails);
                    break;
                case 3:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the date from which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    start = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 4:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the date to which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    end = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 5:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the minimal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    linesMin = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
                case 6:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]the maximal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    linesMax = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
                case 7:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]part of the comment[/] you want to search for. ", TextUIOptions.Optional);
                    if (temp == "e")
                    {
                        break;
                    }
                    comments = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 8:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]minimal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    durationMin = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 9:
                    temp = UserInterface.DisplayTextUI("Please insert [Blue]maximal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                    if (temp == "e")
                    {
                        break;
                    }
                    durationMax = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
                case 10:
                    dynamic resultSortingOrder = UserInterface.DisplayEnumSelectionUI("Show records that were [purple]timer tracked (true)[/], or were [purple]inserted manually (false)[/]: ", typeof(MenuSelections.TimerTracked), Color.MediumPurple);
                    if (!(resultSortingOrder is int))
                    {
                        wasTimerTracked = Enum.GetName(resultSortingOrder) == "TimerTracked" ? "True" : "False";
                    }
                    else
                    {
                        wasTimerTracked = null;
                    }
                    break;
                case 11:
                    returnToMenu = true;
                    runFilterMenuLoop = false;
                break;
            }
        }
        return null;
    }
    internal static SortingDetails SortingMenu(SortingDetails previousDetails)
    {
        bool inSortingMenu = true;
        SortingDetails sortingDetails = TemporaryData.lastFilter.SortingDetails;

        while (inSortingMenu)
        {
            Dictionary<string, string> filteringSelections = new Dictionary<string, string>()
            {
                { Enum.GetName(typeof(MenuSelections.SortingMenu), (MenuSelections.SortingMenu)1), sortingDetails.SortOrder == null ? null : Regex.Replace(Enum.GetName(sortingDetails.SortOrder.GetType(), sortingDetails.SortOrder), @"(?<=[A-Za-z])([A-Z])", @" $1")},
                { Enum.GetName(typeof(MenuSelections.SortingMenu), (MenuSelections.SortingMenu)2), sortingDetails.SortBy == null ? null : Regex.Replace(Enum.GetName(sortingDetails.SortBy.GetType(), sortingDetails.SortBy), @"(?<=[A-Za-z])([A-Z])", @" $1")},
            };
            int? userSelection = UserInterface.DisplaySelectionUIWithUserInputs("Please [purple]select your sorting options:[/]", typeof(MenuSelections.SortingMenu), Color.LightSeaGreen, filteringSelections, "[green]Execute[/]", false);

            switch (userSelection)
            {
                case -1:
                    TemporaryData.lastFilter.SortingDetails = sortingDetails;
                    return sortingDetails;
                case 0:
                    sortingDetails.SortBy = null;
                    sortingDetails.SortOrder = null;
                    break;
                case 1:
                    dynamic resultSortingOrder = UserInterface.DisplayEnumSelectionUI("Please select your sorting order: ", typeof(MenuSelections.SortingOrder), Color.LightSeaGreen);
                    if (!(resultSortingOrder is int))
                    {
                        sortingDetails.SortOrder = resultSortingOrder;
                    }
                    break;
                case 2:
                    dynamic resultSortingBy = UserInterface.DisplayEnumSelectionUI("Please select your sorting option: ", typeof(MenuSelections.SortingBy), Color.LightSeaGreen);
                    if (!(resultSortingBy is int))
                    {
                        sortingDetails.SortBy = resultSortingBy;
                    }
                    break;
                case 3:
                    return new SortingDetails()
                    {
                        SortBy = null,
                        SortOrder = null
                    };
            }
        }
        return null;
    }
    internal static void GenerateReport()
    {
        throw new NotImplementedException();
    }
}
