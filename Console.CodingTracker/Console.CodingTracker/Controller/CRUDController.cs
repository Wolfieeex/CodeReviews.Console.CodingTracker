using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;
using System.Timers;
using System.Text.RegularExpressions;
using System.Globalization;
using Console.CodingTracker.Controller.ScreenMangers;

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

            bool valuesNotInserted = true;
            bool blockEndOption = false;
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                valuesNotInserted = false;
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
                userOption = UserInterface.DisplaySelectionUIWithUserInputs($"Track your [violet]new session{(valuesNotInserted ? " (please fill in all non-optional values to proceed)" : "")}:[/]", typeof(MenuSelections.TrackNewSession), Color.DodgerBlue1, dic, "[green]AddRecord[/]", blockEndOption, "[red]The start date of your session must be earlier than the end date of your session:[/]");
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
                        string duration = SQLCommands.InjectRecord(new CodingSession(DateTime.Now.ToString($"dd/MM/yyyy, HH:mm"), 
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
                                            SQLCommands.InjectRecord(new CodingSession((sessionStart + TimeSpan.FromSeconds(secondsPassed)).ToString($"dd/MM/yyyy, HH:mm"),
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

            List<CodingSession> sessions = SQLCommands.GetRecords(Filter);
            if (sessions != null && sessions.Any())
            {
                Tables.DrawDatatable(sessions, Filter.ViewOptions);
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
            List<CodingSession> sessions = SQLCommands.GetRecords(filters);

            if (sessions == null || !sessions.Any())
            {
                System.Console.Clear();
                AnsiConsole.Markup("No records to update with given filters. Press any [blue]key to go back to filter menu[/]: ");
                System.Console.ReadKey();
                continue;
            }

            bool[] viewOptions = filters.ViewOptions;
            Tables.DrawDatatable(sessions, viewOptions);
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
            List<CodingSession> selectedSessions = new List<CodingSession>();
            foreach (int i in indexNumbers)
            {
                if (!selectedSessions.Contains(sessions[i - 1]))
                {
                    selectedSessions.Add(sessions[i - 1]);
                }
            }

            updateMenuRun = UpdateMenu(selectedSessions);
        }
    }
    internal static bool UpdateMenu(List<CodingSession> sessions)
    {
        bool updateWhile = true;
        while (updateWhile)
        {
            System.Console.Clear();
            Tables.DrawDatatable(sessions, new bool[] { true, true, true, true, true, true, true, true });
            int? userOption = UserInterface.DisplaySelectionUI($"{(sessions.Count == 1 ? "" : "Multi - update ([yellow]all selected records will be updated at the same time[/]). ")}[blue]Please make your selection:[/]", typeof(MenuSelections.UpdateMenu), Color.Orange4);

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
                    foreach (CodingSession s in sessions)
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
                    foreach (CodingSession s in sessions)
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
                    foreach (CodingSession s in sessions)
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
                    foreach (CodingSession s in sessions)
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
        bool runDeleteMenu = true;
        while (runDeleteMenu)
        {
            System.Console.Clear();

            bool quitMenu = false;
            FilterDetails filters = FilterRecords("You are currently using [green]delete record method[/]. ", ref quitMenu);
            if (quitMenu)
            {
                break;
            }
            List<CodingSession> sessions = SQLCommands.GetRecords(filters);

            if (sessions == null || !sessions.Any())
            {
                System.Console.Clear();
                AnsiConsole.Markup("No records to update with given filters. Press any [blue]key to go back to filter menu[/]: ");
                System.Console.ReadKey();
                continue;
            }

            bool[] viewOptions = filters.ViewOptions;
            Tables.DrawDatatable(sessions, viewOptions);
            System.Console.WriteLine();
            int reason = 0;

            // This to be transferred into UserInterface class- this is not a controller, this belongs to View Model
            string userInput = AnsiConsole.Prompt(
                new TextPrompt<string>("\nPlese [blue]select record(s) you would like to delete by choosing their index number(s)[/]. Please separate [yellow]multiple records[/] by adding \",\" between them, e.g. [green]1[/]  or  [green]23,58[/]  or  [green]8, 34, 8[/]. You can also insert [red]\"E\" to return to previous menu:[/] ")
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
                               .Select(x => int.Parse(x)).ToArray();
            List<CodingSession> selectedSessions = new List<CodingSession>();
            foreach (int i in indexNumbers)
            {
                if (!selectedSessions.Contains(sessions[i - 1]))
                {
                    selectedSessions.Add(sessions[i - 1]);
                }
            }

            runDeleteMenu = DeletionMenu(selectedSessions);
        }
    }
    internal static bool DeletionMenu(List<CodingSession> sessions)
    {
        bool deletionWhile = true;
        while (deletionWhile)
        {
            System.Console.Clear();
            Tables.DrawDatatable(sessions, new bool[] { true, true, true, true, true, true, true, true });
            int? userOption = UserInterface.DisplaySelectionUI($"{(sessions.Count == 1 ? "" : "Multi - deletion ([red]all selected records will be deleted at the same time[/]). ")}[blue]Please make your selection:[/]", typeof(MenuSelections.DeletionMenu), Color.Orange1);

            string demonstrative = sessions.Count > 1 ? "all the selected sessions" : "that session";
            switch (userOption)
            {
                case 0:
                    bool confirmation = UserInterface.DisplayConfirmationSelectionUI($"[red]Are you sure you want to delete {demonstrative}?[/]", "Yes", "No");
                    if (confirmation)
                    {
                        SQLCommands.DeleteRecords(sessions.Select(x => x.Key).ToList());
                        AnsiConsole.Markup($"Deletion of {sessions.Count} record{(sessions.Count > 1 ? "s" : "")} completed. [blue]Press any key[/] to return to the previous menu: ");
                        System.Console.ReadKey();
                        return true;
                    }
                    break;
                case 1:
                    return true;
                case 2:
                    return false;
            }
        }
        return false;
    }
    internal static FilterDetails FilterRecords(string preTitle, ref bool returnToMenu)
    {
        FilterDetails filterDetails = new FilterDetails()
        {
            SortingDetails = TemporaryData.lastFilter.SortingDetails,
            ViewOptions = TemporaryData.lastFilter.ViewOptions,
            FromDate = TemporaryData.lastFilter.FromDate,
            ToDate = TemporaryData.lastFilter.ToDate,
            MinLines = TemporaryData.lastFilter.MinLines,
            MaxLines = TemporaryData.lastFilter.MaxLines,
            Comment = TemporaryData.lastFilter.Comment,
            MinDuration = TemporaryData.lastFilter.MinDuration,
            MaxDuration = TemporaryData.lastFilter.MaxDuration,
            WasTimerTracked = TemporaryData.lastFilter.WasTimerTracked
        };
        SortingDetails sortingDetails = TemporaryData.lastFilter.SortingDetails;
        
        bool runFilterMenuLoop = true;
        while (runFilterMenuLoop)
        {
            string sortingDetailsString = sortingDetails.SortBy == null || sortingDetails.SortOrder == null ? null : sortingDetails.SortOrder.ToString() + ", " + Regex.Replace(sortingDetails.SortBy.ToString(), @"(?<=[A-Za-z])([A-Z])", @" $1");
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)2), sortingDetailsString},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)3), filterDetails.FromDate},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)4), filterDetails.ToDate},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)5), filterDetails.MinLines},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)6), filterDetails.MaxLines},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)7), filterDetails.Comment},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)8), filterDetails.MinDuration},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)9), filterDetails.MaxDuration},
                { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)10), filterDetails.WasTimerTracked}
            };

            System.Console.Clear();

            string reason = "";
            bool shouldBlock = false;
            CheckFilterConditions(filterDetails, ref reason, ref shouldBlock);

            FilterScreenManager.BasicFilterMenu(preTitle, ref returnToMenu, ref filterDetails, ref sortingDetails, ref runFilterMenuLoop, dic, reason, shouldBlock);
        }
        return filterDetails;
    }
    private static void CheckFilterConditions(FilterDetails filterDetails, ref string reason, ref bool shouldBlock)
    {
        if (!String.IsNullOrEmpty(filterDetails.FromDate) && !String.IsNullOrEmpty(filterDetails.ToDate))
        {
            DateTime dateStart = DateTime.Parse(filterDetails.FromDate);
            DateTime dateEnd = DateTime.Parse(filterDetails.ToDate);

            if (dateEnd < dateStart)
            {
                reason += "[red]The start date of your session must be before the end date of your session.[/]\n";
                shouldBlock = true;
            }
        }
        if (!String.IsNullOrEmpty(filterDetails.MaxLines) && !String.IsNullOrEmpty(filterDetails.MinLines))
        {

            if (Int32.Parse(filterDetails.MaxLines) < Int32.Parse(filterDetails.MinLines))
            {
                reason += "[red]Minimal number of lines cannot exceed maximal lines search.[/]\n";
                shouldBlock = true;
            }
        }
        if (!String.IsNullOrEmpty(filterDetails.MaxDuration) && !String.IsNullOrEmpty(filterDetails.MinDuration))
        {
            if (TimeSpan.ParseExact(filterDetails.MaxDuration, @"d\ hh\:mm", new CultureInfo("en-GB"), TimeSpanStyles.None) < TimeSpan.ParseExact(filterDetails.MinDuration, @"d\ hh\:mm", new CultureInfo("en-GB"), TimeSpanStyles.None))
            {
                reason += "[red]Your maximal session time needs to be longer than the minimal session time.[/]\n";
                shouldBlock = true;
            }
        }
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
        ReportSettings reportSettings = TemporaryData.reportSettings;
        FilterDetails filterDetails = TemporaryData.lastFilter;
        reportSettings.FilterDetails = filterDetails;

        string? reportOptionsString = null;
        string? dataOptionsString = null;
        string? PeriodSelectionString = null;

        bool loopReportMenu = true;
        while (loopReportMenu)
        {
            bool wasFilterSelected = true;
            if (filterDetails.FromDate == null && filterDetails.ToDate == null && filterDetails.MinLines == null && filterDetails.MaxLines == null && filterDetails.Comment == null && filterDetails.MinDuration == null && filterDetails.MaxDuration == null && filterDetails.WasTimerTracked == null)
            {
                wasFilterSelected = false;
            }
            try
            {
                if (Enum.GetNames(typeof(ReportOptions)).Length != reportSettings.ReportOptions.Length)
                {
                    throw new DataMisalignedException("ReportOptions Enum must have the same length as reportSettings.ReportOptions array length.");
                }
                if (Enum.GetNames(typeof(SummationOptions)).Length != reportSettings.DataOptions.Length)
                {
                    throw new DataMisalignedException("SummationOptions Enum must have the same length as reportSettings.DataOptions array length.");
                }
                
                if (reportSettings.ReportOptions != null)
                {
                    reportOptionsString = "";
                    int counter = 0;
                    foreach (string s in Enum.GetNames(typeof(ReportOptions)))
                    {
                        if (reportSettings.ReportOptions[counter])
                        {
                            reportOptionsString += Regex.Replace(s, @"(?<=[a-z])([A-Z]{1})", " $1");
                            reportOptionsString += ", ";
                        }
                        counter++;
                    }
                    reportOptionsString = reportOptionsString.Remove(reportOptionsString.Length - 2, 2);
                }
                else
                {
                    reportOptionsString = null;
                }
                if (reportSettings.DataOptions != null)
                {
                    dataOptionsString = "";
                    int counter = 0;
                    foreach (string s in Enum.GetNames(typeof(SummationOptions)))
                    {
                        if (reportSettings.DataOptions[counter])
                        {
                            dataOptionsString += Regex.Replace(s, @"(?<=[a-z])([A-Z]{1})", " $1");
                            dataOptionsString += ", ";
                        }
                        counter++;
                    }
                    dataOptionsString = dataOptionsString.Remove(dataOptionsString.Length - 2, 2);
                }
                else
                {
                    dataOptionsString = null;
                }
                if (reportSettings.Period != null)
                {
                    PeriodSelectionString = "";
                    PeriodSelectionString += Enum.GetName(typeof(ReportSortationPeriod), reportSettings.Period);

                    if (reportSettings.SortationYear != null)
                    {
                        PeriodSelectionString += ", " + reportSettings.SortationYear.ToString();
                    }
                    if (reportSettings.SortationMonth != null)
                    {
                        PeriodSelectionString += ", " + Enum.GetName(typeof(Months), reportSettings.SortationMonth);
                    }
                }
            }
            catch
            {
                System.Console.ReadKey();
            }

            Dictionary<string, string> reportMenuDic = new Dictionary<string, string>()
            {
                { Enum.GetName((ReportMenu)0), (wasFilterSelected ? "Filter(s) selected" : "No filters")},
                { Enum.GetName((ReportMenu)1), reportOptionsString },
                { Enum.GetName((ReportMenu)2), dataOptionsString },
                { Enum.GetName((ReportMenu)3), PeriodSelectionString }
            };

            int? userInput = UserInterface.DisplaySelectionUIWithUserInputs("\nYou are currently in the [purple]report generation menu[/]. Please [purple]select your report settings: [/]", typeof(ReportMenu), Color.MediumPurple2_1, reportMenuDic, "Run report", false);

            bool[] tempOptions;
            switch (userInput)
            {
                case -1:
                    reportSettings.FilterDetails = filterDetails;
                    TemporaryData.reportSettings = reportSettings;
                    TemporaryData.lastFilter = filterDetails;

                    Dictionary<string, List<string>> DurationTable = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> LinesTable = new Dictionary<string, List<string>>();
                    SQLCommands.CalculateReport(reportSettings, out DurationTable, out LinesTable);
                    Tables.DrawReportTable(reportSettings, DurationTable, LinesTable);
                    break;
                case 0:
                    bool runFilterMenu = true;
                    while (runFilterMenu)
                    {
                        Dictionary<string, string> filterDic = new Dictionary<string, string>()
                        {
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)1), filterDetails.FromDate},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)2), filterDetails.ToDate},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)3), filterDetails.MinLines},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)4), filterDetails.MaxLines},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)5), filterDetails.Comment},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)6), filterDetails.MinDuration},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)7), filterDetails.MaxDuration},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)8), filterDetails.WasTimerTracked}
                        };
                        System.Console.Clear();
                        bool shouldBlock = false;
                        string reason = "";
                        CheckFilterConditions(filterDetails, ref reason, ref shouldBlock);
                        FilterScreenManager.ReportFilterMenu("Records used to calculate your report will be [purple]selected by your filters. [/]", ref filterDetails, ref runFilterMenu, filterDic, reason, shouldBlock);
                    }
                    break;
                case 1:
                    tempOptions = reportSettings.ReportOptions;
                    UserInterface.DisplayMultiselectionUI("Select [purple]data to display for your report[/]:", typeof(ReportOptions), ref tempOptions);
                    reportSettings.ReportOptions = tempOptions;
                    break;
                case 2:
                    tempOptions = reportSettings.DataOptions;
                    UserInterface.DisplayMultiselectionUI("Select [purple]variables by which your report will calculated[/]:", typeof(SummationOptions), ref tempOptions);
                    reportSettings.DataOptions = tempOptions;
                    break;
                case 3:
                    PeriodSelectionMenu(ref reportSettings);
                    break;
                case 4:
                    loopReportMenu = false;
                    TemporaryData.reportSettings = reportSettings;
                    TemporaryData.lastFilter = filterDetails;
                    break;
            }
        }
    }
    internal static void PeriodSelectionMenu(ref ReportSettings reportSettings)
    {
        bool runPeriodMenu = true;
        while (runPeriodMenu)
        {
            List<string> years = SQLCommands.ReturnAllRecordedYears();

            var enumReturn = UserInterface.DisplayEnumSelectionUI("Select by [purple]which period you would like to generate your report:[/]", typeof(ReportSortationPeriod), Color.Purple4);
            if (enumReturn is not int)
            {
                if (enumReturn == ReportSortationPeriod.Yearly)
                {
                    reportSettings.Period = enumReturn;
                    reportSettings.SortationYear = null;
                    reportSettings.SortationMonth = null;
                    runPeriodMenu = false;
                    break;
                }

                bool runYearSelectionMenu = true;
                while (runYearSelectionMenu)
                {
                    string year = UserInterface.DisplayStringListSelectionUI("Select [purple]by which year[/] the report should be built:", years, Color.Purple3);
                    if (year == "0")
                    {
                        runYearSelectionMenu = false;
                        continue;
                    }
                    if (year == "-1")
                    {
                        runYearSelectionMenu = false;
                        runPeriodMenu = false;
                        break;
                    }
                    if (enumReturn == ReportSortationPeriod.Monthly || enumReturn == ReportSortationPeriod.Weekly)
                    {
                        reportSettings.Period = enumReturn;
                        reportSettings.SortationYear = int.Parse(year);
                        reportSettings.SortationMonth = null;
                        runPeriodMenu = false;
                        break;
                    }

                    bool runMonthSelectionMenu = true;
                    while (runMonthSelectionMenu)
                    {
                        List<int> months = SQLCommands.ReturnRecordedMonthsForYear(int.Parse(year));
                        for (int i = 0; i < months.Count; i++)
                        {
                            months[i] -= 1;
                        }
                        List<Months> enumMonths = new List<Months>();
                        for (int i = 0; i < months.Count; i++)
                        {
                            enumMonths.Add((Months)months[i]);
                        }

                        string monthSelection = UserInterface.DisplayStringListSelectionUI("Select [purple]by which month[/] the report should be built:", enumMonths.Select(s => Enum.GetName(typeof(Months), s)).ToList(), Color.Purple3);
                        if (monthSelection == "0")
                        {
                            runMonthSelectionMenu = false;
                            continue;
                        }

                        runYearSelectionMenu = false;
                        runMonthSelectionMenu = false;
                        runPeriodMenu = false;

                        if (monthSelection == "-1")
                        {
                            break;
                        }
                        reportSettings.Period = enumReturn;
                        reportSettings.SortationYear = int.Parse(year);
                        reportSettings.SortationMonth = (Months)Enum.Parse(typeof(Months), monthSelection);
                        runPeriodMenu = false;
                    }
                }
            }
            else
            {
                runPeriodMenu = false;
            }
        }
        
    }
    internal static bool IndexCheck(string index, int sessionsLength, ref int reason)
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
