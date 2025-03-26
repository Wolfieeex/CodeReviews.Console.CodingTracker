using Console.CodingTracker.View;
using System.Text.RegularExpressions;
using Console.CodingTracker.Model;
using Spectre.Console;

namespace Console.CodingTracker.Controller.SQL;

public enum MenuGoalType
{
    ReturnToPreviousMenu,
    ToCommitCertainAmountOfTimeForProgramming,
    ToProduceCertainAmountOfCodingLines
}

internal enum GoalLimit
{
    BackToPreviousMenu,
    Custom,
    Days3,
    Week1,
    Weeks2,
    Month1,
    Months3,
    Months6,
    Year1,
}

internal static class GoalSettings
{
    
    private static Color titleColor = Color.LightGreen;
    private static Color mainColor = Color.Chartreuse1;
    private static Color inputColor = Color.Yellow4_1;

    private static string titleColorHex = $"[#{titleColor.ToHex()}]";
    private static string mainColorHex = $"[#{mainColor.ToHex()}]";
    private static string inputColorHex = $"[#{inputColor.ToHex()}]";

    internal static void GoalMenu()
    {
        bool runMenuLoop = true ;
        while (runMenuLoop)
        {
			System.Console.Clear();
			string title = $"You are now in the {titleColorHex}goal menu[/]. Please select one of the options below: ";

            int? userInput = UserInterface.DisplaySelectionUI(title, typeof(MenuSelections.GoalSetterMenu), mainColor);

            switch (userInput)
            {
                case 0:
                    runMenuLoop = false ;
                    continue;
                case 1:
                    SetNewGoal();
                    break;
                case 2:
                    ViewPreviousGoals();
                    break;
                case 3:
                    DeleteGoals();
                    break;
            }
        }
    }

    private static void SetNewGoal()
    {
        bool runTypeSelectorMenu = true;
        while (runTypeSelectorMenu)
        {
            System.Console.Clear();
            int? userInputType = UserInterface.DisplaySelectionUI($"Happy to see that {titleColorHex}you are ready to set your new goal![/]\nPlease select what is your goal: ", typeof(MenuGoalType), mainColor);
            if (userInputType! == 0)
            {
                break;
            }

            bool runTimeLimitMenu = true ;
            while (runTimeLimitMenu)
            {
				System.Console.Clear();
				int? userInputTimeLimit = UserInterface.DisplaySelectionUI($"That's a good one! Please select {titleColorHex}the time limit for your goal[/]: ", typeof(GoalLimit), mainColor);

                if (userInputTimeLimit! == 0)
                {
                    runTypeSelectorMenu= false ;
                    break;
                }

                
                TimeSpan timeLimit = new TimeSpan(0);
                switch (userInputTimeLimit)
                {
                    case 1:
                        dynamic timeLimitCustom = UserInterface.DisplayTextUI($"Please select a custom time as your limit to complete your goal in the {inputColorHex}\"d hh:mm\"[/] format. ", TextUIOptions.TimeSpanOnly, titleColor, goalSetterTitle: true);
                        if (timeLimitCustom != null)
                        {
                            string timeLimitString = timeLimitCustom.ToString();
                            if (timeLimitString.ToLower() == "e" || timeLimitString == "")
                            {
                                runTimeLimitMenu = false;
                                continue;
                            }
                            timeLimit = TimeSpan.Parse(timeLimitString.Replace(' ', '.'));
                        }
                        else
                        {
                            runTimeLimitMenu = false;
							continue;
                        }
                        break;
                    case 2:
                        timeLimit = new TimeSpan(3, 0, 0, 0);
                        break;
                    case 3:
                        timeLimit = new TimeSpan(7, 0, 0, 0);
                        break;
                    case 4:
                        timeLimit = new TimeSpan(14, 0, 0, 0);
                        break;
                    case 5:
                        timeLimit = DateTime.Now.AddMonths(1) - DateTime.Now;
                        break;
                    case 6:
                        timeLimit = DateTime.Now.AddMonths(3) - DateTime.Now;
                        break;
                    case 7:
                        timeLimit = DateTime.Now.AddMonths(6) - DateTime.Now;
                        break;
                    case 8:
                        timeLimit = DateTime.Now.AddYears(1) - DateTime.Now;
                        break;
                }

                bool runGoalAmountMenu = true;
                while (runGoalAmountMenu)
                {
                    
                    string timeLimitInString = Regex.Replace(timeLimit.ToString(), @"(d\+)\s", match =>
                    {
                        return match.Groups[1].Value == "1" ? "1 day" : match.Groups[1].Value + " days";
                    });

					switch (userInputType)
					{
						case 1:
							dynamic commitAmount = UserInterface.DisplayTextUI($"Please {inputColorHex}input total time[/] you want to commit into programming in the {inputColorHex}\"d hh:ss\"[/] format in the {timeLimitInString} timeframe: ", TextUIOptions.TimeSpanOnly, titleColor, goalSetterTitle: true);
                            if (commitAmount != null)
                            {
                                string commitAmountString = commitAmount.ToString();
                                if (commitAmountString.ToLower() == "e" || commitAmountString == "")
                                {
                                    runTimeLimitMenu = false;
                                    continue;
                                }
                                Goal goal = new Goal(commitAmountString, timeLimitInString);
                                goal.AddGoalToDatabase();
                            }
                            else
                            {
								runTimeLimitMenu = false;
                                continue;
							}
						    break;
						case 2:
							commitAmount = UserInterface.DisplayTextUI($"Please {inputColorHex}input number of lines[/] you want to produce in the {timeLimitInString} timeframe: ", TextUIOptions.NumbersOnly, titleColor, goalSetterTitle: true);
							if (commitAmount != null)
							{
								string commitLines = commitAmount.ToString();
								if (commitLines.ToLower() == "e" || commitLines == "")
								{
									runTimeLimitMenu = false;
									continue;
								}
                                int linesGoalInt = int.Parse(commitLines);
								Goal goal = new Goal(linesGoalInt, timeLimitInString);
								goal.AddGoalToDatabase();
							}
							else
							{
								runTimeLimitMenu = false;
								continue;
							}
							break;
					}
                    System.Console.Clear();
                    AnsiConsole.Markup($"Your new {titleColorHex}goal has been added[/]! You are on a good track friend!\n{inputColorHex}Press any button[/] to return to goal settings menu: ");
                    System.Console.ReadKey();
                    runGoalAmountMenu = false;
                    runTimeLimitMenu = false;
				}
            }
        }
    }
    private static void ViewPreviousGoals()
    {

    }

    private static void DeleteGoals()
    {
        
    }
}
