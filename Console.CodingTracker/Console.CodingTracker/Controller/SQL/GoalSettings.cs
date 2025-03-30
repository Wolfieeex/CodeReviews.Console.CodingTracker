﻿using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using System.Text.RegularExpressions;
using Spectre.Console;
using Microsoft.Data.Sqlite;

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
                                continue;
                            }
                            timeLimit = TimeSpan.Parse(timeLimitString.Replace(' ', '.'));
                        }
                        else
                        {
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
                    System.Console.Clear();

					switch (userInputType)
					{
						case 1:
							dynamic commitAmount = UserInterface.DisplayTextUI($"Please {inputColorHex}input total time[/] you want to commit into programming in the {inputColorHex}\"d hh:ss\"[/] format in the {ViewTimeSpan(timeLimit.ToString())} timeframe: ", TextUIOptions.TimeSpanOnly, titleColor, goalSetterTitle: true);
                            if (commitAmount != null)
                            {
                                string commitAmountString = commitAmount.ToString();
                                if (commitAmountString.ToLower() == "e" || commitAmountString == "")
                                {
									runGoalAmountMenu = false;
                                    continue;
                                }

								TimeSpan commitAmountTimeSpan = TimeSpan.Parse(commitAmountString.Replace(' ', '.'));
								Goal goal = new Goal(DateTime.Now, timeLimit, commitAmountTimeSpan, commitAmountTimeSpan);
                                goal.AddGoalToDatabase();
                            }
                            else
                            {
								runGoalAmountMenu = false;
                                continue;
							}
						    break;
						case 2:
							commitAmount = UserInterface.DisplayTextUI($"Please {inputColorHex}input number of lines[/] you want to produce in the {ViewTimeSpan(timeLimit.ToString())} timeframe: ", TextUIOptions.NumbersOnly, titleColor, goalSetterTitle: true);
							if (commitAmount != null)
							{
								string commitLines = commitAmount.ToString();
								if (commitLines.ToLower() == "e" || commitLines == "")
								{
									runGoalAmountMenu = false;
									continue;
								}
                                int linesGoalInt = int.Parse(commitLines);
								Goal goal = new Goal(DateTime.Now, timeLimit, linesGoalInt, linesGoalInt);
								goal.AddGoalToDatabase();
							}
							else
							{
								runGoalAmountMenu = false;
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

    private static string ViewTimeSpan(string initialString)
    {
		initialString = Regex.Replace(initialString.ToString(), @"(d\+)\s|\.", match =>
		{
			return match.Groups[1].Value == "1" ? "1 day " : match.Groups[1].Value + " days ";
		});
        return Regex.Match(initialString, @".+?:\d{2}").Value;
	}
    private static void ViewPreviousGoals()
    {
        bool runViewMenu = true;
        while (runViewMenu)
        {
            System.Console.Clear();
            int? userInput = UserInterface.DisplaySelectionUI($"Here, you can {titleColorHex}view your goals[/]. First, select one of the options below: ", typeof(MenuSelections.GoalViewerMenu), inputColor);

            string statusFilter = null;
            switch (userInput)
            {
                case 0:
                    runViewMenu = false;
                    continue;
                case 1:
                    statusFilter = ((GoalStatus)1).ToString();
					break;
				case 2:
					statusFilter = ((GoalStatus)0).ToString();
					break;
				case 3:
					statusFilter = ((GoalStatus)2).ToString();
					break;
				case 4:
					bool confirm = UserInterface.DisplayConfirmationSelectionUI($"This action will [red]delete all previous history[/], including both failed and completed goals. Are you sure you want to continue?", "delete", "no", inputColor);
                    if (confirm)
                    {
                        DeleteGoalHistory();
                    }
					continue;
			}

			System.Console.Clear();

            List<Goal> goals = new List<Goal>();
			using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
			{
				conn.Open();
				SqliteCommand comm = conn.CreateCommand();
				comm.CommandText = $"SELECT * FROM {Settings.GoalDatabaseName} WHERE Status = '{statusFilter ??= "-1"}'";
                SqliteDataReader reader = comm.ExecuteReader();

                if (!reader.HasRows)
                {
                    AnsiConsole.Markup($"No records found. Please {titleColorHex}press any button[/] to return to the previous menu: ");
                    System.Console.ReadKey();
                    continue;
                }

                while (reader.Read())
                {
                    bool isInt = int.TryParse(reader.GetString(5), out int _);
                    if (isInt)
                    {
                        DateTime startDate = DateTime.Parse(reader.GetString(3));
                        TimeSpan timeToComplete = DateTime.Parse(reader.GetString(4)) - DateTime.Now;
                        int startGoal = reader.GetInt32(5);
                        int goalLeft = reader.GetInt32(6);
						goals.Add(new Goal(startDate, timeToComplete, startGoal, goalLeft));
					}
					else
                    {
						DateTime startDate = DateTime.Parse(reader.GetString(3));
						TimeSpan timeToComplete = DateTime.Parse(reader.GetString(4)) - DateTime.Now;
						TimeSpan startGoal = TimeSpan.Parse(reader.GetString(5));
						TimeSpan goalLeft = TimeSpan.Parse(reader.GetString(6));
						goals.Add(new Goal(startDate, timeToComplete, startGoal, goalLeft));
					}
                }

                // Table creation:
                Table table = new Table();
                table.AddColumns(new string[] { "Index", "Goal", "Status", "Time left", "Amount of work left" });
                for (int i = 0; i < goals.Count; i++)
                {
                    table.AddRow(goals[i].SetTableRow(i));
                }

                // Table formatting:
				foreach (var col in table.Columns)
				{
					col.Width(col.Width + 3);
					col.RightAligned().Padding(1, 0);
					col.NoWrap();
				}
				table.Columns[0].Width(table.Columns[0].Width + 3).LeftAligned().Padding(1, 0);
				table.Centered();
				table.Border = TableBorder.Rounded;
				table.ShowRowSeparators();
				table.BorderColor(Color.BlueViolet);

                // Table title:
				string statusView = Regex.Replace(statusFilter, @"(.+)([A-Z])", match =>
                {
                    return match.Groups[1].Value + " " + match.Groups[2].Value;
                });
                System.Console.WriteLine(statusView);
				table.Title($"Goals that are {statusView}:", new Style().Foreground(Color.DeepPink1));
				
                // Table view:
				System.Console.Clear();
				AnsiConsole.Write(table);
				AnsiConsole.Write(new Rule($"Press any button to [#{inputColor.ToHex()}]return to the previous menu:[/]").Centered());
				System.Console.ReadKey();
				System.Console.Clear();
			}
		}
    }

	private static void DeleteGoalHistory()
	{
		System.Console.Clear();
		using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
		{
			conn.Open();
			SqliteCommand comm = conn.CreateCommand();
			comm.CommandText = $"DELETE FROM {Settings.GoalDatabaseName} WHERE Status = 'Completed' OR Status = 'Failed'";
			comm.ExecuteNonQuery();
		}
		AnsiConsole.Markup($"All previous records have been erased. {titleColorHex}Press any button[/] to return to the previous menu: ");
		System.Console.ReadKey();
	}

    // Regular update timer

    // Update after tracking a new session NOT IF injecting or updating. Only tracking by timer counts.

    private static void DeleteGoals()
    {
        
    }
}
