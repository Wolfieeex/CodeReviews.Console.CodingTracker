using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using System.Text.RegularExpressions;
using Spectre.Console;
using Microsoft.Data.Sqlite;
using System.Configuration;
using System.Reflection.PortableExecutable;

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
		UpdateForFailedGoals(GoalFailure);

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

                    DateTime deadline = DateTime.Now + timeLimit;

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
								Goal goal = new Goal(DateTime.Now, deadline, commitAmountTimeSpan, commitAmountTimeSpan, "N/A");
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
								Goal goal = new Goal(DateTime.Now, deadline, linesGoalInt, linesGoalInt, "N/A");
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
            bool tableRendered = RenderGoalTable(statusFilter!);

            if (tableRendered)
            {
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
			comm.CommandText = $"DELETE FROM {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} WHERE Status = 'Completed' OR Status = 'Failed'";
			comm.ExecuteNonQuery();
		}
		AnsiConsole.Markup($"All previous records have been erased. {titleColorHex}Press any button[/] to return to the previous menu: ");
		System.Console.ReadKey();
	}
    internal static void UpdateForFailedGoals(Action<List<Goal>, List<Goal>, GoalStatus, Color> readFailedGoals)
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
			string command = $"SELECT * FROM {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} WHERE Status = 'InProgress'";
			SqliteCommand comm = new SqliteCommand(command, conn);
			SqliteDataReader reader = comm.ExecuteReader();

			// SqliteDatabase Id for each record
			Dictionary<int, Goal> goals = new Dictionary<int, Goal>();
			Dictionary<int, Goal> failedGoals = new Dictionary<int, Goal>();
			Dictionary<int, Goal> closeShaveGoals = new Dictionary<int, Goal>();

			while (reader.Read())
			{
				if (reader.GetString(1) == "Time")
					goals.Add(reader.GetInt32(0),
						new Goal(DateTime.Parse(reader.GetString(3)),
					        DateTime.Parse(reader.GetString(4)),
					        TimeSpan.Parse(reader.GetString(5)),
						    TimeSpan.Parse(reader.GetString(6)),
							reader.GetString(7)));
				else
					goals.Add(reader.GetInt32(0),
						new Goal(DateTime.Parse(reader.GetString(3)),
							DateTime.Parse(reader.GetString(4)),
					        reader.GetInt32(5),
						    reader.GetInt32(6),
							reader.GetString(7)));
			}
			reader.Close();

			foreach (var goal in goals)
            {
                if (goal.Value.EndTime - DateTime.Now - goal.Value.ProgrammingTimeLeft < TimeSpan.Zero)
                {                   
                    failedGoals.Add(goal.Key, goal.Value);
                    command = $"UPDATE {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} SET Status = 'Failed' WHERE id = {goal.Key}";
                    comm.CommandText = command;
                    comm.ExecuteNonQuery();
                }
                else if (goal.Value.EndTime - DateTime.Now < TimeSpan.FromDays(1))
                {
					closeShaveGoals.Add(goal.Key, goal.Value);
				}
            }

            readFailedGoals(failedGoals.Values.ToList(), closeShaveGoals.Values.ToList(), GoalStatus.Failed, inputColor);
		}
    }
    internal static void UpdateGoals(int numberOfLines, TimeSpan timeCommited, Action<List<Goal>, List<Goal>, GoalStatus, Color> actionDel)
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();

            string command = $"SELECT * FROM {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} WHERE Status = 'InProgress'";
            SqliteCommand comm = new SqliteCommand(command, conn);
            SqliteDataReader reader = comm.ExecuteReader();

            // SqliteDatabase Id for each record
            Dictionary<int, Goal> goals = new Dictionary<int, Goal>();
			Dictionary<int, Goal> completedGoals = new Dictionary<int, Goal>();
			Dictionary<int, Goal> closeToCompleteGoals = new Dictionary<int, Goal>();

			while (reader.Read())
            {
                if (TimeSpan.TryParse(reader.GetString(5), out _))
                    goals.Add(reader.GetInt32(0),
                        new Goal(DateTime.Parse(reader.GetString(3)),
						DateTime.Parse(reader.GetString(4)),
                        TimeSpan.Parse(reader.GetString(5)),
                        TimeSpan.Parse(reader.GetString(6))));
                else
                    goals.Add(reader.GetInt32(0),
						new Goal(DateTime.Parse(reader.GetString(3)),
						DateTime.Parse(reader.GetString(4)),
						reader.GetInt32(5),
						reader.GetInt32(6)));
			}
            reader.Close();

            foreach (var goal in goals)
            {
				Goal completeGoal;

				if (goal.Value.GoalType == GoalType.Time)
                {
                    goal.Value.ProgrammingTimeLeft -= timeCommited;
					completeGoal = new Goal(goal.Value.StartTime, goal.Value.EndTime, goal.Value.StartProgrammingTime, goal.Value.ProgrammingTimeLeft);

					if (completeGoal.ProgrammingTimeLeft == TimeSpan.Zero)
                    {
                        completedGoals.Add(goal.Key, completeGoal);

                        command = $"UPDATE {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} " +
                            $"SET [Goal Amount Left] = '{TimeSpan.Zero.ToString()}', Status = 'Completed', [Finish Time] = {DateTime.Now} WHERE Id = {goal.Key}";
                        comm = new SqliteCommand(command);
                        comm.ExecuteNonQuery();
                    }
                    else if (completeGoal.ProgrammingTimeLeft <= TimeSpan.FromHours(2))
                    {
						closeToCompleteGoals.Add(goal.Key, completeGoal);
					}
                }
                else
                {
					goal.Value.LinesLeft -= numberOfLines;
					completeGoal = new Goal(goal.Value.StartTime, goal.Value.EndTime, goal.Value.StartLines, goal.Value.LinesLeft);

					if (completeGoal.LinesLeft == 0)
                    {
						completedGoals.Add(goal.Key, completeGoal);

                        command = $"UPDATE {ConfigurationManager.AppSettings.Get("DatabaseName")} " +
                            $"SET [Goal Amount Left] = '0', Status = 'Completed', [Finish Time] = {DateTime.Now} WHERE Id = {goal.Key}";
                        comm = new SqliteCommand(command);
                        comm.ExecuteNonQuery();
                    }
                    else if (completeGoal.LinesLeft <= 200)
                    {
						closeToCompleteGoals.Add(goal.Key, completeGoal);
					}
				}
            }

            actionDel(completedGoals.Values.ToList(), closeToCompleteGoals.Values.ToList(), GoalStatus.Completed, titleColor);
            UpdateForFailedGoals(actionDel);
		}
    }
    private static void DeleteGoals()
    {
        bool runDeleteMenu = true;
        while (runDeleteMenu)
        {
            System.Console.Clear();
            bool wasSuccessful = RenderGoalTable(((GoalStatus)1).ToString());

            if (!wasSuccessful)
                break;

            bool userSelectionforDeletion = true;
            while (userSelectionforDeletion)
            {
                AnsiConsole.Write(new Rule("-").Centered());

                int recordsCount = 0;
                using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
                {
                    conn.Open();
                    SqliteCommand comm = conn.CreateCommand();
                    comm.CommandText = $"SELECT COUNT(*) FROM {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} WHERE Status = 'InProgress'";
                    recordsCount = Convert.ToInt32(comm.ExecuteScalar());
                }

                string[] indexNumbers = Array.Empty<string>();
                string reason = "";

				string userInput = AnsiConsole.Prompt(new TextPrompt<string>($"Please insert {titleColorHex}index numbers you want to remove seperated by commas[/]. For example, you can input {mainColorHex}2[/] or {mainColorHex}2, 15[/] or {mainColorHex}1,2,3[/]. You can also input {mainColorHex}nothing[/] or {mainColorHex}'E'[/] to return to the previous menu: ")
                    .Validate((string s) => s.ToLower() switch
                    {
                        { } when IndexCheck(s, recordsCount, out indexNumbers, out reason) => ValidationResult.Success(),
						null or "" or "e" => ValidationResult.Success(),
                        _ => ValidationResult.Error($"{reason}")
                    })
                    .AllowEmpty()
                    );

                if (String.IsNullOrEmpty(userInput))
                {
                    runDeleteMenu = false;
                    break;
                }
                else if (userInput.ToLower() == "e" || userInput == "")
                {
					runDeleteMenu = false;
					break;
				}

                System.Console.Clear();

                string pluralForm = $"Are you sure you want to {titleColorHex}delete {indexNumbers.Length} goals that you started tracking[/]? This operation [red]cannot be reversed[/] and the statuses of those goals will change to \"Failed\".";
                string singularForm = $"Are you sure you want to {titleColorHex}delete {indexNumbers.Length} goal that you started tracking[/]? This operation [red]cannot be reversed[/] and the status of that goal will change to \"Failed\".";
				string promptVersion = indexNumbers.Length > 1 ? pluralForm : singularForm;

                bool userIsSure = UserInterface.DisplayConfirmationSelectionUI(promptVersion,
                    "Delete", "Back", inputColor);

                if (userIsSure)
                {
                    using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
                    {
                        conn.Open();

                        // Unfortunately, I need to pull IDs here... OR NOT!

						List<int> numList = new List<int>();
						foreach (string i in indexNumbers)
                        {
                            int num = int.Parse(i);
                            numList.Add(num);
                        }

                        numList.Sort();
                        numList.Reverse();

						foreach (int i in numList)
						{
							SqliteCommand comm = conn.CreateCommand();
                            comm.CommandText = $"UPDATE {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} SET Status = 'Failed', [Finish Time] = 'DEL' WHERE Status = 'InProgress' AND Id = ( SELECT Id FROM {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} WHERE Status = 'InProgress' LIMIT 1 OFFSET {i - 1} )";
                            comm.ExecuteNonQuery();
                        }
                    }
                        runDeleteMenu = false;
				}
                break;
			}
		}
	}
    /// <summary>
    /// </summary>
    /// <param name="status"></param>
    /// <param name="deletionMenuVersion"></param>
    /// <returns>Bool - did table render succeed?</returns>
    private static bool RenderGoalTable(string status, bool deletionMenuVersion = false)
    {
		System.Console.Clear();

		List<Goal> goals = new List<Goal>();
		using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
		{
			conn.Open();
			SqliteCommand comm = conn.CreateCommand();
			comm.CommandText = $"SELECT * FROM {ConfigurationManager.AppSettings.Get("GoalDatabaseName")} WHERE Status = '{status ??= "-1"}'";
			SqliteDataReader reader = comm.ExecuteReader();

			if (!reader.HasRows)
			{
                if (deletionMenuVersion)
                {
					AnsiConsole.Markup($"No records to remove at this time. Please {titleColorHex}press any button[/] to return to the previous menu: ");
					System.Console.ReadKey();
					return false;
				}
				else
                {
					AnsiConsole.Markup($"No records found. Please {titleColorHex}press any button[/] to return to the previous menu: ");
					System.Console.ReadKey();
					return false;
				}
			}

			while (reader.Read())
			{
				DateTime startDate = DateTime.Parse(reader.GetString(3));
				DateTime deadline = DateTime.Parse(reader.GetString(4));
				string completionTime = reader.GetString(7);

				bool isInt = int.TryParse(reader.GetString(5), out int _);
				if (isInt)
				{
					int startGoal = reader.GetInt32(5);
					int goalLeft = reader.GetInt32(6);
					goals.Add(new Goal(startDate, deadline, startGoal, goalLeft, completionTime));
				}
				else
				{
					TimeSpan timeToComplete = DateTime.Parse(reader.GetString(4)) - DateTime.Now;
					TimeSpan startGoal = TimeSpan.Parse(reader.GetString(5));
					TimeSpan goalLeft = TimeSpan.Parse(reader.GetString(6));
					goals.Add(new Goal(startDate, deadline, startGoal, goalLeft, completionTime));
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
			string statusView = Regex.Replace(status, @"(.+)([A-Z])", match =>
			{
				return match.Groups[1].Value + " " + match.Groups[2].Value;
			});
			System.Console.WriteLine(statusView);
			table.Title($"Goals that are {statusView}:", new Style().Foreground(Color.DeepPink1));

			// Table view:
			System.Console.Clear();
			AnsiConsole.Write(table);

            return true;
		}
	}
	internal static bool RenderGoalTable(List<Goal> goals, string tableTitle)
	{
		System.Console.Clear();

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

		table.Title(tableTitle, new Style().Foreground(Color.DeepPink1));

		// Table view:
		System.Console.Clear();
		AnsiConsole.Write(table);

		return true;
		
	}
	private static bool IndexCheck(string s, int recordNumber, out string[] indexNumbers, out string reason)
    {
        reason = $"This is not in the correct format. Only use {inputColorHex}integer or integers separated by commas.[/]";

		indexNumbers = Array.Empty<string>();

        if (!(Regex.Match(s, @"^(\d+)(,{1}\d+)*$").Success))
        {
            return false;
        }

        s.Trim().Trim(',');
        indexNumbers = s.Split(',');

        List<string> indexNumbersNoRepetition = new List<string>();
        foreach (string ind in indexNumbers)
        {
            if (int.Parse(ind) < 1 || int.Parse(ind) > recordNumber)
            {
                indexNumbers = Array.Empty<string>();
				reason = $"The chosen index numbers must {inputColorHex}lie between 1 and {recordNumber}[/].";
				return false;
            }
            if (!indexNumbersNoRepetition.Contains(ind))
            {
                indexNumbersNoRepetition.Add(ind);
            }
		}

        indexNumbers = indexNumbersNoRepetition.ToArray();

        return true;
    }
    private static void GoalFailure(List<Goal> failed, List<Goal> closeToFail, GoalStatus status, Color color)
    {
		System.Console.Clear();

		failed ??= new List<Goal>();
		closeToFail ??= new List<Goal>();

		string hexColor = "[#" + color.ToHex() + "]";

        if (failed.Count > 0)
        {
            string singularForm = $"Oooops! Unfortunately, you have {hexColor}failed one of your Goals...[/]";
            string pluralForm = $"Oooops! Unfortunately, you have {hexColor}failed {failed.Count} of your goals...[/]";
            string tableTitle = failed.Count == 1 ? singularForm : pluralForm;
            RenderGoalTable(failed, tableTitle);

            if (closeToFail.Count == 0)
            {
                System.Console.WriteLine();
                AnsiConsole.Write(new Rule("-"));
                AnsiConsole.Markup($"Press any button {inputColorHex}to continue:[/] ");
                System.Console.ReadKey();
            }
            else
            {
				System.Console.WriteLine();
				AnsiConsole.Write(new Rule("-"));
				AnsiConsole.Markup($"Press any button {inputColorHex}to continue:[/] ");
				System.Console.ReadKey();
				bool userViewChoice = UserInterface.DisplayConfirmationSelectionUI(
	            "Would you like to view goals for which you don't have much time left?", "yes", "no", color);
				if (userViewChoice)
				{
					RenderGoalTable(closeToFail, $"Goal{(closeToFail.Count > 1 ? "s" : "")} that {(closeToFail.Count > 1 ? "are" : "is")} close to be failed");
					System.Console.WriteLine();
					AnsiConsole.Write(new Rule("-"));
					AnsiConsole.Markup($"Press any button {inputColorHex}to continue:[/] ");
					System.Console.ReadKey();
				}
			}
		}
        else if (closeToFail.Count > 0)
        {
			bool userViewChoice = UserInterface.DisplayConfirmationSelectionUI(
				"Would you like to view goals for which you don't have much time left?", "yes", "no", color);
			if (userViewChoice)
			{
				RenderGoalTable(closeToFail, $"Goal{(closeToFail.Count > 1 ? "s" : "")} that {(closeToFail.Count > 1 ? "are" : "is")} close to be failed");
				System.Console.WriteLine();
				AnsiConsole.Write(new Rule("-"));
				AnsiConsole.Markup($"Press any button {inputColorHex}to continue:[/] ");
				System.Console.ReadKey();
			}
		}
    }
}
