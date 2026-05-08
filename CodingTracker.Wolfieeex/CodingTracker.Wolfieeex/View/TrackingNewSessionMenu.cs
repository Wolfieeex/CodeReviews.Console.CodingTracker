using Spectre.Console;
using CodingTracker.Wolfieeex.Model;
using System.Diagnostics;
using System.Timers;
using System.Text.RegularExpressions;

namespace CodingTracker.Wolfieeex.View;

internal class TrackingNewSessionMenu : Menu
{
    protected override string title => "Welcome to recording menu. Here you can start the timer to track a new coding session. Shall we start?";
    protected override Type selectionEnum => typeof(RecordSessionStartMenu);
    public TrackingNewSessionMenu(Color color) : base(color) { }
    public override void DisplayMenu()
    {
        Console.Clear();

        bool trackNewSessionLoop = true;
        while (trackNewSessionLoop)
        {
            // Initial menu
            Console.Clear();
            RecordSessionStartMenu userInput = (RecordSessionStartMenu)DisplayOptions();
            Console.Clear();

            if (userInput == RecordSessionStartMenu.ReturnToMainMenu)
                return;

            //Set the timer and stopwatch
            Stopwatch stopwatch = new Stopwatch();
            System.Timers.Timer timer = new System.Timers.Timer(100);

            DateTime sessionStart = DateTime.Now;
            int secondsPassed = 0;
            int previousMilisecondsPassed = 0;

            DisplayTimer(secondsPassed, menuColors.mainColor);
            timer.Elapsed += TimerEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            stopwatch.Start();
            timer.Start();

            bool trackerOn = true;
            while (trackerOn)
            {
                bool sessionDiscarded = false;
                userInput = UserInterface.DisplaySelectionUI(timer.Enabled ? $"{titleColorHex}Your session is in progress:[/]" : $"{titleColorHex}Your session is[/] {inputColorHex}paused:[/]", timer.Enabled ? typeof(MenuSelections.RecordSessionRecording) : typeof(MenuSelections.RecordSessionPause), mainColor);

                switch ((int)userInput)
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