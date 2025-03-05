using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;
using System.Timers;
using System.Text.RegularExpressions;
using System.Globalization;
using Console.CodingTracker.Controller.ScreenMangers;
using Console.CodingTracker.Controller.SQL;

namespace Console.CodingTracker.Controller.CRUD;

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
                        string duration = Crud.InjectRecord(new CodingSession(DateTime.Now.ToString($"dd/MM/yyyy, HH:mm"),
                                                                               DateTime.Now.ToString($"dd/MM/yyyy, HH:mm"),
                                                                               start,
                                                                               end,
Helpers.CalculateDuration(start, end).ToString(),
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
                                    "r" => ValidationResult.Success(),
                                    "d" => ValidationResult.Success(),
                                    string when int.TryParse(s, out _) && int.Parse(s) > 0 => ValidationResult.Success(),
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
                                        "r" => ValidationResult.Success(),
                                        "d" => ValidationResult.Success(),
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
                                            Crud.InjectRecord(new CodingSession((sessionStart + TimeSpan.FromSeconds(secondsPassed)).ToString($"dd/MM/yyyy, HH:mm"),
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

            void TimerEvent(object source, ElapsedEventArgs e)
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
                Filter = FilterController.FilterRecords("You are currently using [green]view previous sessions method[/]. ", ref returnToMenu);
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

            List<CodingSession> sessions = Crud.GetRecords(Filter);
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
            FilterDetails filters = FilterController.FilterRecords("You are currently using [green]update record method[/]. ", ref quitMenu);
            if (quitMenu)
            {
                break;
            }
            List<CodingSession> sessions = Crud.GetRecords(filters);

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
                    "e" => ValidationResult.Success(),
                    string when !IndexCheck(s, sessions.Count, ref reason) => ValidationResult.Error($"\n{(reason == 0 ? "You can only [blue]input index numbers you want to update separated by commas[/] or [red]\"E\" to return to a previous menu[/]." : "You can only select index " + (sessions.Count == 1 ? "number 1" : " numbers from 1 to " + sessions.Count))}\n"),
                    _ => ValidationResult.Success()
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
                    if (string.IsNullOrEmpty(temp))
                    {
                        break;
                    }
                    Crud.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp, MenuSelections.UpdateMenu.UpdateStartDate);
                    foreach (CodingSession s in sessions)
                    {
                        s.StartDate = temp.Trim();
                        s.LastUpdateDate = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
                    }
                    List<string> durations = Helpers.GetDurations(sessions.Select(x => x.Key).ToList());
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
                    if (string.IsNullOrEmpty(temp))
                    {
                        break;
                    }
                    Crud.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp, MenuSelections.UpdateMenu.UpdateEndDate);
                    foreach (CodingSession s in sessions)
                    {
                        s.EndDate = temp.Trim();
                        s.LastUpdateDate = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
                    }
                    durations = Helpers.GetDurations(sessions.Select(x => x.Key).ToList());
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

                    Crud.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp == null ? "" : temp, MenuSelections.UpdateMenu.UpdateNumberOfLines);
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

                    Crud.UpdateRecords(sessions.Select(x => x.Key).ToList(), temp == null ? "" : temp, MenuSelections.UpdateMenu.UpdateComments);
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
            FilterDetails filters = FilterController.FilterRecords("You are currently using [green]delete record method[/]. ", ref quitMenu);
            if (quitMenu)
            {
                break;
            }
            List<CodingSession> sessions = Crud.GetRecords(filters);

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
                    "e" => ValidationResult.Success(),
                    string when !IndexCheck(s, sessions.Count, ref reason) => ValidationResult.Error($"\n{(reason == 0 ? "You can only [blue]input index numbers you want to update separated by commas[/] or [red]\"E\" to return to a previous menu[/]." : "You can only select index " + (sessions.Count == 1 ? "number 1" : " numbers from 1 to " + sessions.Count))}\n"),
                    _ => ValidationResult.Success()
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
                        Crud.DeleteRecords(sessions.Select(x => x.Key).ToList());
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
