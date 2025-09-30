using Microsoft.Data.Sqlite;

namespace CodingTracker.Wolfieeex.Model;

public enum GoalStatus
{
    Completed,
    InProgress,
    Failed,
}

public enum GoalType
{
    Time,
    Lines
}

internal class Goal
{
	public GoalType GoalType { get; init; }
	public DateTime StartTime { get; init; }
	public DateTime EndTime { get; init; }
    private TimeSpan _timeLeft;
    public TimeSpan TimeLeft
    {
        get
        {
            return (EndTime - DateTime.Now) < TimeSpan.Zero ? TimeSpan.Zero : (EndTime - DateTime.Now);
        }
    }
	public TimeSpan StartProgrammingTime { get; init; }
	private TimeSpan _programmingTimeLeft;
	public TimeSpan ProgrammingTimeLeft { get => _programmingTimeLeft; set {
            if (value <= TimeSpan.Zero)
				_programmingTimeLeft = TimeSpan.Zero;
            else
				_programmingTimeLeft = value; 
		} 
	}
    public int StartLines { get; init; }
	private int _linesLeft;
	public int LinesLeft { get => _linesLeft; set
        {
            if (value <= 0)
				_linesLeft = 0;
            else
				_linesLeft = value;
        } 
	}
    public string FinishTime { get; set; }
    private string Status { get
        {
            if (FinishTime == "DEL")
            {
                return "Failed";
            }
            if (GoalType == GoalType.Time)
            {
                if (ProgrammingTimeLeft == TimeSpan.Zero)
                {
                    return "Completed";
                }
            }
            else
            {
				if (LinesLeft == 0)
				{
					return "Completed";
				}
			}
            if (EndTime - DateTime.Now - ProgrammingTimeLeft < TimeSpan.Zero)
            {
				return "Failed";
			}

			return "In Progress";
        } 
	}

    /// <summary>
    /// Creates a goal with time of programming span type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(DateTime startTime, DateTime deadline, TimeSpan startProgrammingTime, TimeSpan programmingTimeLeft)
    {
        this.StartTime = startTime;
		EndTime = deadline;
		this.StartProgrammingTime = startProgrammingTime;
		this.ProgrammingTimeLeft = programmingTimeLeft;
		GoalType = GoalType.Time;
        FinishTime = "N/A";
    }

	public Goal(DateTime startTime, DateTime deadline, TimeSpan startProgrammingTime, TimeSpan programmingTimeLeft, string finishingTime)
	{
		this.StartTime = startTime;
		EndTime = deadline;
		this.StartProgrammingTime = startProgrammingTime;
		this.ProgrammingTimeLeft = programmingTimeLeft;
		GoalType = GoalType.Time;
        FinishTime = finishingTime;
	}

	/// <summary>
	/// Creates a goal with number of lines produced type, the 2nd parameter accepts the time in which goal needs to be completed.
	/// </summary>
	public Goal(DateTime startTime, DateTime deadline, int startLines, int linesLeft)
    {
		this.StartTime = startTime;
		EndTime = deadline;
        this.StartLines = startLines;
		this.LinesLeft = linesLeft;
		GoalType = GoalType.Lines;
		FinishTime = "N/A";

		StartProgrammingTime = TimeSpan.Zero;
		ProgrammingTimeLeft = TimeSpan.Zero;
	}
	public Goal(DateTime startTime, DateTime deadline, int startLines, int linesLeft, string finishingTime)
	{
		this.StartTime = startTime;
		EndTime = deadline;
		this.StartLines = startLines;
		this.LinesLeft = linesLeft;
		GoalType = GoalType.Lines;
		FinishTime = finishingTime;

        StartProgrammingTime = TimeSpan.Zero;
        ProgrammingTimeLeft = TimeSpan.Zero;
	}

	public void AddGoalToDatabase()
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
			string goalDatabaseName = System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName");

			conn.Open();
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO {goalDatabaseName}(Goal, Status, [Start Date], [End Date], [Start Goal Amount], [Goal Amount Left], [Finish Time]) VALUES (@Type, 'InProgress', @Start, @End, @GoalStartAmount, @GoalAmountLeft, @Finishtime)";
			cmd.Parameters.AddWithValue("@Type", GoalType.ToString());
			cmd.Parameters.AddWithValue("@Start", StartTime.ToString());
			cmd.Parameters.AddWithValue("@End", EndTime.ToString());
			cmd.Parameters.AddWithValue("@GoalStartAmount", GoalType == GoalType.Lines ? StartLines : StartProgrammingTime);
			cmd.Parameters.AddWithValue("@GoalAmountLeft", GoalType == GoalType.Lines ? LinesLeft : ProgrammingTimeLeft);
			cmd.Parameters.AddWithValue("@Finishtime", $"N/A");
			cmd.ExecuteNonQuery();
		}
    }
    public string[] SetTableRow(int index)
    {
        string[] row = new string[5];

        if (Status == "Completed")
        {
			row[0] = (index + 1).ToString();
			row[1] = GoalType == GoalType.Lines ? "To produce a certain amount of coding lines" : "To program for a certain amount of time";
			row[2] = TimeSpanView(DateTime.Parse(FinishTime) - StartTime);
			row[3] = GoalType == GoalType.Lines ? StartLines.ToString() + " lines" : TimeSpanView(StartProgrammingTime);
			row[4] = FinishTime;
		}
        else if (Status == "Failed")
        {
			row[0] = (index + 1).ToString();
			row[1] = GoalType == GoalType.Lines ? "To produce a certain amount of coding lines" : "To program for a certain amount of time";
			row[2] = GoalType switch
			{
				GoalType.Lines => ((StartLines - (double)LinesLeft + 0.0000001f) / StartLines).ToString("p2"),
				GoalType.Time => ((StartProgrammingTime - ProgrammingTimeLeft + TimeSpan.FromSeconds(1)) / StartProgrammingTime).ToString("p2"),
				_ => "Unspecified"
			};
			row[3] = GoalType == GoalType.Lines ? LinesLeft.ToString() + " lines" : TimeSpanView(ProgrammingTimeLeft);
			row[4] = FinishTime switch
			{
				"DEL" => "Deleted by user",
				"DDL" => "Deadline reached",
				"IMP" => "Not enough time to complete",
				_ => "Unknown reason"
			};
		}
        else
        {
			row[0] = (index + 1).ToString();
			row[1] = GoalType == GoalType.Lines ? "To produce a certain amount of coding lines" : "To program for a certain amount of time";
			row[2] = EndTime.ToString();
			row[3] = TimeSpanView((EndTime - DateTime.Now) < TimeSpan.Zero ? TimeSpan.Zero : (EndTime - DateTime.Now));
			row[4] = GoalType == GoalType.Lines ? LinesLeft.ToString() + " lines" : TimeSpanView(ProgrammingTimeLeft);
		}
			
        return row;
    }
	public string[] SetTableColumn()
	{
        if (Status == "Completed")
            return new string[]{ "Index", "Goal", "Completed in", "Goal achieved", "Completion date" };
        else if (Status == "Failed")
			return new string[]{ "Index", "Goal", "Completion progress", "Goal amount left", "Reason for fail" };
        else
			return new string[] { "Index", "Goal", "Deadline date", "Time left", "Amount of work left" };
	}
	private string TimeSpanView(TimeSpan timeSpan)
    {
        string days;
        if (timeSpan.Days == 0)
        {
            days = "";
		}
        else
        {
			days = timeSpan.Days == 1 ? "1 day " : $"{timeSpan.Days} days ";
		}
        return $"{days}{timeSpan.ToString("hh\\:mm\\:ss")}";
	}
}
