using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;
using System.Timers;
using System.Text.RegularExpressions;
using Console.CodingTracker.Controller.SQL;
using System.Diagnostics;

namespace Console.CodingTracker.Controller.CRUD;

internal class CRUDController
{
    internal static void AddNewSessionManually()
    {
        Color titleColor = Color.Orange4_1;
        Color mainColor = Color.Yellow4;
        Color inputColor = Color.LightGoldenrod3;

        string titleColorHex = $"[#{titleColor.ToHex()}]";
        string mainColorHex = $"[#{mainColor.ToHex()}]";
        string inputColorHex = $"[#{inputColor.ToHex()}]";

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
                userOption = UserInterface.DisplaySelectionUIWithUserInputs($"Track your {titleColorHex}new session[/]{(valuesNotInserted ? $" (please fill in all non-optional values to proceed). {inputColorHex}Please note, that injected sections will not count towards your goals. You need to track your new session by timer in this app to progress your goals[/]" : "")}:", typeof(MenuSelections.TrackNewSession), titleColor, mainColor, inputColor , dic, $"{Settings.optionalsCompleted}AddRecord[/]", blockEndOption, $"{Settings.optionalsNotCompleted}The start date of your session must be earlier than the end date of your session[/]");
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
                        temp = UserInterface.DisplayTextUI($"Please insert {titleColorHex}the start of the session[/] in {titleColorHex}\"dd/mm/yyyy, hh:mm\" format[/]. ", TextUIOptions.DateOnly, mainColor);
                        if (temp.ToLower() == "e")
                        {
                            break;
                        }
                        start = temp;
                        start.Trim();
                        break;
                    case 1:
                        temp = UserInterface.DisplayTextUI($"Please insert {inputColorHex}the end of the session[/] in {titleColorHex}\"dd/mm/yyyy, hh:mm\" format[/]. ", TextUIOptions.DateOnly, mainColor);
                        if (temp.ToLower() == "e")
                        {
                            break;
                        }
                        end = temp;
                        end.Trim();
                        break;
                    case 2:
                        temp = UserInterface.DisplayTextUI($"Please insert {titleColorHex}number of lines you produced[/] during your session. ", TextUIOptions.NumbersOnlyOptional, mainColor);
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
                        temp = UserInterface.DisplayTextUI($"Please insert {titleColorHex}any comments[/] you want to add. ", TextUIOptions.Optional, mainColor);
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

						bool addAnotherRecord = UserInterface.DisplayConfirmationSelectionUI($"Coding session of duration [#{inputColor.ToHex()}]{duration} has been added![/]\nWould you like to [#{titleColor.ToHex()}]add another record[/], or [#{mainColor.ToHex()}]return to the main menu[/]?", "Add", "Return", inputColor);

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
        Color titleColor = Color.SeaGreen2;
        Color mainColor = Color.SeaGreen1;
        Color inputColor = Color.SeaGreen1_1;

        string titleColorHex = $"[#{titleColor.ToHex()}]";
        string mainColorHex = $"[#{mainColor.ToHex()}]";
        string inputColorHex = $"[#{inputColor.ToHex()}]";

        System.Console.Clear();
        bool trackNewSessionLoop = true;
        while (trackNewSessionLoop)
        {
            int? option = UserInterface.DisplaySelectionUI($"Start tracking your {titleColorHex}new session[/]:", typeof(MenuSelections.RecordSessionStartMenu), mainColor);

            if (option == 1)
            {
                return;
            }
            System.Console.Clear();

            int secondsPassed = 0;
            int previousMilisecondsPassed = 0;

            DisplayTimer(secondsPassed, mainColor);


            DateTime sessionStart = DateTime.Now;

            Stopwatch stopwatch = new Stopwatch();
            System.Timers.Timer timer = new System.Timers.Timer(100);

            timer.Elapsed += TimerEvent;
            timer.AutoReset = true;

            timer.Enabled = true;
            stopwatch.Start();
            timer.Start();

            bool trackerOn = true;
            while (trackerOn)
            {
                bool sessionDiscarded = false;
                option = UserInterface.DisplaySelectionUI(timer.Enabled ? $"{titleColorHex}Your session is in progress:[/]" : $"{titleColorHex}Your session is[/] {inputColorHex}paused:[/]", timer.Enabled ? typeof(MenuSelections.RecordSessionRecording) : typeof(MenuSelections.RecordSessionPause), mainColor);

                switch (option)
                {
                    case 0:
                        TimerPauseStart();
						break;
                    case 1:
						TimerPauseStart(true);

						if (UserInterface.DisplayConfirmationSelectionUI($"Are you sure you want to {inputColorHex}discard this session?[/]", "yes", "no", inputColor))
                        {
                            timer.Close();
                            trackerOn = false;
                            System.Console.Clear();
                            break;
                        }
                        System.Console.Clear();

						TimerPauseStart();
						break;
                    case 2:
						TimerPauseStart(true);

						System.Console.Clear();
                        if (!(UserInterface.DisplayConfirmationSelectionUI($"Are you sure you want to {inputColorHex}end this session?[/]", "no", "yes", inputColor)))
                        {


                            System.Console.Clear();
                            string input = AnsiConsole.Prompt(
                                new TextPrompt<string>($"Please {mainColorHex}insert the number of lines produced[/]. If you changed your mind and want to {inputColorHex}discard this timer[/], insert {inputColorHex}\"D\"[/]. If you want to {mainColorHex}continue tracking[/], insert {mainColorHex}\"R\"[/]: ")
                                .Validate((s) => s.ToLower() switch
                                {
                                    "r" => ValidationResult.Success(),
                                    "d" => ValidationResult.Success(),
                                    string when int.TryParse(s, out _) && int.Parse(s) > 0 => ValidationResult.Success(),
                                    _ => ValidationResult.Error($"Please {mainColorHex}enter \"R\" to resume[/], {inputColorHex}\"D\" to discard[/], or {mainColorHex}valid number to continue[/]: ")
                                })
                                );
                            switch (input.ToLower())
                            {
                                case "r":
                                    break;
                                case "d":
                                    System.Console.Clear();
                                    if (UserInterface.DisplayConfirmationSelectionUI($"Are you sure you want to {inputColorHex}discard this session?[/]", "yes", "no", inputColor))
                                    {
                                        sessionDiscarded = true;
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
                                    new TextPrompt<string>($"Please {mainColorHex}insert any comments you want to add[/]. If you changed your mind and want to {inputColorHex}discard this timer[/], insert {inputColorHex}\"D\"[/]. If you want to {mainColorHex}continue tracking[/], insert {mainColorHex}\"R\"[/]: ")
                                    .Validate((s) => s.ToLower() switch
                                    {
                                        "r" => ValidationResult.Success(),
                                        "d" => ValidationResult.Success(),
                                        string => ValidationResult.Success(),
                                        _ => ValidationResult.Error($"Please enter {mainColorHex}\"R\" to resume[/], {inputColorHex}\"D\" to discard[/], or a {mainColorHex}comment to continue[/]: ")
                                    })
                                    );

                                    switch (input.ToLower())
                                    {
                                        case "r":
                                            break;
                                        case "d":
                                            System.Console.Clear();
                                            if (UserInterface.DisplayConfirmationSelectionUI($"Are you sure you want to {inputColorHex}discard this session?[/]", "yes", "no", inputColor))
                                            {
                                                sessionDiscarded = true;
                                                timer.Close();
                                                trackerOn = false;
                                                System.Console.Clear();
                                                break;
                                            }
                                            break;
                                        default:
                                            TimeSpan duration = TimeSpan.FromSeconds(secondsPassed);

											Crud.InjectRecord(new CodingSession((sessionStart + duration).ToString($"dd/MM/yyyy, HH:mm"),
                                                                                (sessionStart + duration).ToString($"dd/MM/yyyy, HH:mm"),
                                                                                sessionStart.ToString($"dd/MM/yyyy, HH:mm"),
                                                                                (sessionStart + duration).ToString($"dd/MM/yyyy, HH:mm"),
                                                                                TimeSpan.FromSeconds(secondsPassed).ToString(),
                                                                                numberOfLines,
                                                                                input,
                                                                                true));

                                            GoalSettings.UpdateGoals(numberOfLines, duration, ShowUserGoalUpdates);
                                            System.Console.Clear();

                                            if (!UserInterface.DisplayConfirmationSelectionUI($"Coding session of duration {titleColorHex}{TimeSpan.FromSeconds(secondsPassed).ToString()} has been added![/] Would you like to {inputColorHex}start another session[/], or {titleColorHex}return to the main menu[/]?:", "Start", "Return", inputColor))
                                            {
                                                sessionDiscarded = true;
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
                        if (!sessionDiscarded)
                        {
                            System.Console.Clear();
							TimerPauseStart();
						}
                        break;
                }
            }

            void TimerEvent(object source, ElapsedEventArgs e)
            {
                int currentMilisecondsPassed =  previousMilisecondsPassed + (int)stopwatch.ElapsedMilliseconds;

                if (currentMilisecondsPassed / 1000 != secondsPassed)
                {
					secondsPassed = currentMilisecondsPassed / 1000;
                    DisplayTimer(secondsPassed, mainColor);
				}
			}

            void TimerPauseStart(bool alwaysSetToPause = false)
            {
                if (stopwatch.IsRunning && timer.Enabled)
                {
                    stopwatch.Stop();
                    timer.Stop();
                    
					previousMilisecondsPassed = previousMilisecondsPassed + (int)stopwatch.ElapsedMilliseconds;
					if (previousMilisecondsPassed / 1000 != secondsPassed)
					{
						DisplayTimer(previousMilisecondsPassed / 1000, mainColor);
					}
				}
                else if (!stopwatch.IsRunning && !timer.Enabled)
                {
                    if (!alwaysSetToPause)
                    {
						DisplayTimer(previousMilisecondsPassed / 1000, mainColor);
						stopwatch.Restart();
						timer.Start();
					}
                }
                else
                {
                    throw new ApplicationException("Both stopwatch and timer need to be running or not running at the same time. One of them rioted.");
                }
			}
        }

        void DisplayTimer(int seconds, Color color)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            bool isMatch = Regex.IsMatch(timeSpan.ToString(), @"(?<=^1)\.");

            System.Console.Clear();
            AnsiConsole.Write(
            new FigletText(Regex.Replace(timeSpan.ToString(), @"\.", isMatch ? " day, " : " days, "))
                .Centered()
                .Color(color));
        }
    }
    internal static void ViewPreviousSessions()
    {
        Color titleColor = Color.MediumOrchid;
        Color mainColor = Color.MediumPurple2;
        Color inputColor = Color.Fuchsia;

        FilterDetails Filter = null;
        bool returnToMenu = false;

        while (!returnToMenu)
        {
            try
            {
                Filter = FilterController.FilterRecords($"You are currently using [#{titleColor.ToHex()}]view previous sessions method[/]. ", ref returnToMenu, titleColor, mainColor, inputColor);
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
                AnsiConsole.Write(new Markup($"\n[#{titleColor.ToHex()}]Press any key[/] to return to previous menu:").Centered());
            }
            else if (Filter == null)
            {
                AnsiConsole.Write(new Markup($"[#{titleColor.Blend(Color.Red, 0.5f).ToHex()}]No records found[/]. [#{titleColor.ToHex()}]Press any key[/] to return to previous menu:"));
            }
            else
            {
                AnsiConsole.Write(new Markup($"[#{titleColor.Blend(Color.Red, 0.2f).ToHex()}]No records found with selected filters[/]. [#{titleColor.ToHex()}]Press any key[/] to return to previous menu:"));
            }
            System.Console.ReadKey();
        }
    }
    internal static void UpdateSessionDetails()
    {
        Color titleColor = Color.DarkOrange3;
        Color mainColor = Color.Orange1;
        Color inputColor = Color.Orange3;

        string titleColorHex = $"[#{titleColor.ToHex()}]";
        string mainColorHex = $"[#{mainColor.ToHex()}]";
        string inputColorHex = $"[#{inputColor.ToHex()}]";

        bool updateMenuRun = true;
        while (updateMenuRun)
        {
            System.Console.Clear();

            bool quitMenu = false;
            FilterDetails filters = FilterController.FilterRecords($"You are currently using {titleColorHex}update record method[/]. {inputColorHex}Updates will not count towards your goals. [/]", ref quitMenu, titleColor, mainColor, inputColor);
            if (quitMenu)
            {
                break;
            }
            List<CodingSession> sessions = Crud.GetRecords(filters);

            if (sessions == null || !sessions.Any())
            {
                System.Console.Clear();
                AnsiConsole.Markup($"No records to update with given filters. Press any {titleColorHex}key to go back to filter menu[/]: ");
                System.Console.ReadKey();
                continue;
            }

            bool[] viewOptions = filters.ViewOptions;
            Tables.DrawDatatable(sessions, viewOptions);
            System.Console.WriteLine();
            int reason = 0;

            string userInput = AnsiConsole.Prompt(
                new TextPrompt<string>($"\nPlese {titleColorHex}select record(s) you would like to update by choosing their index number(s)[/]. Please separate {titleColorHex}multiple records[/] by adding \",\" between them, e.g. {inputColorHex}1[/]  or  {inputColorHex}23,58[/]  or  {inputColorHex}8, 34, 8[/]. You can also insert {titleColorHex}\"E\" to return to previous menu:[/] ")
                .Validate((s) => s.ToLower() switch
                {
                    "e" => ValidationResult.Success(),
                    string when !IndexCheck(s, sessions.Count, ref reason) => ValidationResult.Error($"\n{(reason == 0 ? $"You can only {titleColorHex}input index numbers you want to update separated by commas[/] or {titleColorHex}\"E\" to return to a previous menu[/]." : "You can only select index " + (sessions.Count == 1 ? "number 1" : " numbers from 1 to " + sessions.Count))}\n"),
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
        Color titleColor = Color.DarkOrange3;
        Color mainColor = Color.Orange1;
        Color inputColor = Color.Orange3;

        string titleColorHex = $"[#{titleColor.ToHex()}]";
        string mainColorHex = $"[#{mainColor.ToHex()}]";
        string inputColorHex = $"[#{inputColor.ToHex()}]";

        bool updateWhile = true;
        while (updateWhile)
        {
            System.Console.Clear();
            Tables.DrawDatatable(sessions, new bool[] { true, true, true, true, true, true, true, true });
            int? userOption = UserInterface.DisplaySelectionUI($"{(sessions.Count == 1 ? "" : $"Multi - update ({titleColorHex}all selected records will be updated at the same time[/]). ")}{titleColorHex}Please make your selection:[/]", typeof(MenuSelections.UpdateMenu), mainColor);

            string temp = "";
            switch (userOption)
            {

                case 0:
                    temp = UserInterface.DisplayTextUI($"Please insert {titleColorHex}the start of the session[/] in {titleColorHex}\"dd/mm/yyyy, hh:mm\"[/] format. ", TextUIOptions.StartDate, mainColor, sessions.Select(x => x.Key).ToList());
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
                    temp = UserInterface.DisplayTextUI($"Please insert {titleColorHex}the end of the session[/] in {titleColorHex}\"dd/mm/yyyy, hh:mm\"[/] format. ", TextUIOptions.EndDate, mainColor, sessions.Select(x => x.Key).ToList());
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
                    temp = UserInterface.DisplayTextUI($"Please insert {titleColorHex}number of lines you produced[/] during your session. ", TextUIOptions.NumbersOnlyOptional, mainColor);
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
                    temp = UserInterface.DisplayTextUI($"Please insert {titleColorHex}any comments[/] you want to add. ", TextUIOptions.Optional, titleColor);
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
        Color titleColor = Color.DarkRed_1;
        Color mainColor = Color.Red3;
        Color inputColor = Color.Red;

        string titleColorHex = $"[#{titleColor.ToHex()}]";
        string mainColorHex = $"[#{mainColor.ToHex()}]";
        string inputColorHex = $"[#{inputColor.ToHex()}]";

        bool runDeleteMenu = true;
        while (runDeleteMenu)
        {
            System.Console.Clear();
            bool quitMenu = false;
            FilterDetails filters = FilterController.FilterRecords($"You are currently using {titleColorHex}delete record method[/]. ", ref quitMenu, titleColor, mainColor, inputColor);
            if (quitMenu)
            {
                break;
            }
            List<CodingSession> sessions = Crud.GetRecords(filters);

            if (sessions == null || !sessions.Any())
            {
                System.Console.Clear();
                AnsiConsole.Markup($"No records to update with given filters. Press any {titleColorHex}key to go back to filter menu[/]: ");
                System.Console.ReadKey();
                continue;
            }

            bool[] viewOptions = filters.ViewOptions;
            Tables.DrawDatatable(sessions, viewOptions);
            System.Console.WriteLine();
            int reason = 0;

            // This to be transferred into UserInterface class- this is not a controller, this belongs to View Model
            string userInput = AnsiConsole.Prompt(
                new TextPrompt<string>($"\nPlese {titleColorHex}select record(s) you would like to delete by choosing their index number(s)[/]. Please separate {inputColorHex}multiple records[/] by adding \",\" between them, e.g. {inputColorHex}1[/]  or  {inputColorHex}23,58[/]  or  {inputColorHex}8, 34, 8[/]. You can also insert {inputColorHex}\"E\" to return to previous menu:[/] ")
                .Validate((s) => s.ToLower() switch
                {
                    "e" => ValidationResult.Success(),
                    string when !IndexCheck(s, sessions.Count, ref reason) => ValidationResult.Error($"\n{(reason == 0 ? $"You can only {mainColorHex}input index numbers you want to update separated by commas[/] or {inputColorHex}\"E\" to return to a previous menu[/]." : "You can only select index " + (sessions.Count == 1 ? "number 1" : " numbers from 1 to " + sessions.Count))}\n"),
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

            runDeleteMenu = DeletionMenu(selectedSessions, titleColor, mainColor, inputColor);
        }
    }
    internal static bool DeletionMenu(List<CodingSession> sessions, Color titleColor, Color mainColor, Color inputColor)
    {
        string titleColorHex = $"[#{titleColor.ToHex()}]";
        string mainColorHex = $"[#{mainColor.ToHex()}]";
        string inputColorHex = $"[#{inputColor.ToHex()}]";

        bool deletionWhile = true;
        while (deletionWhile)
        { 
            System.Console.Clear();
            Tables.DrawDatatable(sessions, new bool[] { true, true, true, true, true, true, true, true });
            int? userOption = UserInterface.DisplaySelectionUI($"{(sessions.Count == 1 ? "" : $"Multi - deletion ({titleColorHex}all selected records will be deleted at the same time[/]). ")}{inputColorHex}Please make your selection:[/]", typeof(MenuSelections.DeletionMenu), Color.Orange1);

            string demonstrative = sessions.Count > 1 ? "all the selected sessions" : "that session";
            switch (userOption)
            {
                case 0:
                    bool confirmation = UserInterface.DisplayConfirmationSelectionUI($"{titleColorHex}Are you sure you want to delete {demonstrative}?[/]", "Yes", "No", Color.Red);
                    if (confirmation)
                    {
                        Crud.DeleteRecords(sessions.Select(x => x.Key).ToList());
                        AnsiConsole.Markup($"Deletion of {sessions.Count} record{(sessions.Count > 1 ? "s" : "")} completed. {inputColorHex}Press any key[/] to return to the previous menu: ");
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
    private static void ShowUserGoalUpdates(List<Goal> primarySubject, List<Goal> secondarySubject, GoalStatus status, Color color)
    {
		System.Console.Clear();

		primarySubject ??= new List<Goal>();
        secondarySubject ??= new List<Goal>();

        string hexColor = "[#" + color.ToHex() + "]";

        if (status == GoalStatus.Completed)
        {
            if (primarySubject.Count > 0)
            {
				string singularForm = $"Congratulations! You have {hexColor}completed one of your Goals![/]";
				string pluralForm = $"Congratulations! You have {hexColor}completed {primarySubject.Count} of your goals![/]";
				string tableTitle = primarySubject.Count == 1 ? singularForm : pluralForm;

				GoalSettings.RenderGoalTable(primarySubject, tableTitle);

                if (secondarySubject.Count > 0)
                {
					System.Console.WriteLine();
					AnsiConsole.Write(new Rule("-"));
					AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
					System.Console.ReadKey();
					bool userchoice = UserInterface.DisplayConfirmationSelectionUI(
	                    "Would you like to also view goals that are close to get completed?", "yes", "no", color);
					if (userchoice)
					{
						GoalSettings.RenderGoalTable(secondarySubject, $"Goal{(secondarySubject.Count > 1 ? "s" : "")} that {(secondarySubject.Count > 1 ? "are" : "is")} close to be completed");
						System.Console.WriteLine();
						AnsiConsole.Write(new Rule("-"));
						AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
						System.Console.ReadKey();
					}
				}
                else
                {
					System.Console.WriteLine();
					AnsiConsole.Write(new Rule("-"));
					AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
					System.Console.ReadKey();
				}
			}
            else if (secondarySubject.Count > 0)
            {
				bool userViewChoice = UserInterface.DisplayConfirmationSelectionUI(
						"Would you like to view goals that are close to get completed?", "yes", "no", color);
				if (userViewChoice)
				{
					GoalSettings.RenderGoalTable(secondarySubject, $"Goal{(secondarySubject.Count > 1 ? "s" : "")} that {(secondarySubject.Count > 1 ? "are" : "is")} close to be completed");
					System.Console.WriteLine();
					AnsiConsole.Write(new Rule("-"));
					AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
					System.Console.ReadKey();
				}
			}
        }
        else
        {
			if (primarySubject.Count > 0)
			{
				string singularForm = $"Oooops! Unfortunately, you have {hexColor}failed one of your Goals...[/]";
				string pluralForm = $"Oooops! Unfortunately, you have {hexColor}failed {primarySubject.Count} of your goals...[/]";
				string tableTitle = primarySubject.Count == 1 ? singularForm : pluralForm;

				GoalSettings.RenderGoalTable(primarySubject, tableTitle);

				if (secondarySubject.Count > 0)
				{
					System.Console.WriteLine();
					AnsiConsole.Write(new Rule("-"));
					AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
					System.Console.ReadKey();
					bool userchoice = UserInterface.DisplayConfirmationSelectionUI(
				        "Would you like to view goals for which you don't have much time left?", "yes", "no", color);
					if (userchoice)
					{
						GoalSettings.RenderGoalTable(secondarySubject, $"Goal{(secondarySubject.Count > 1 ? "s" : "")} that {(secondarySubject.Count > 1 ? "are" : "is")} close to be failed");
						System.Console.WriteLine();
						AnsiConsole.Write(new Rule("-"));
						AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
						System.Console.ReadKey();
					}
				}
				else
				{
					System.Console.WriteLine();
					AnsiConsole.Write(new Rule("-"));
					AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
					System.Console.ReadKey();
				}
			}
			else if (secondarySubject.Count > 0)
			{
				bool userViewChoice = UserInterface.DisplayConfirmationSelectionUI(
				"Would you like to view goals for which you don't have much time left?", "yes", "no", color);
				if (userViewChoice)
				{
					GoalSettings.RenderGoalTable(secondarySubject, $"Goal{(secondarySubject.Count > 1 ? "s" : "")} that {(secondarySubject.Count > 1 ? "are" : "is")} close to be failed");
					System.Console.WriteLine();
					AnsiConsole.Write(new Rule("-"));
					AnsiConsole.Markup($"Press any button {hexColor}to continue:[/] ");
					System.Console.ReadKey();
				}
			}   
		}
	}
}
