using Microsoft.Data.Sqlite;

namespace Console.CodingTracker.Model;

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
    private GoalType goalType;
    private DateTime startTime;
    private DateTime endTime;
	private TimeSpan startProgrammingTime;
	private TimeSpan programmingTimeLeft;
	private int startLines;
	private int linesLeft;

    /// <summary>
    /// Creates a goal with time of programming span type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(DateTime startTime, TimeSpan timeToComplete, TimeSpan startProgrammingTime, TimeSpan programmingTimeLeft)
    {
        this.startTime = startTime;
		endTime = startTime + timeToComplete;
		this.startProgrammingTime = startProgrammingTime;
		this.programmingTimeLeft = programmingTimeLeft;
		goalType = GoalType.Time;
    }

    /// <summary>
    /// Creates a goal with number of lines produced type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(DateTime startTime, TimeSpan timeToComplete, int startLines, int linesLeft)
    {
		this.startTime = startTime;
		endTime = startTime + timeToComplete;
        this.startLines = startLines;
		this.linesLeft = linesLeft;
		goalType = GoalType.Lines;
    }

	public void AddGoalToDatabase()
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO {Settings.GoalDatabaseName}(Goal, Status, [Start Date], [End Date], [Start Goal Amount], [Goal Amount Left]) VALUES (@Type, 'InProgress', @Start, @End, @GoalStartAmount, @GoalAmountLeft)";
			cmd.Parameters.AddWithValue("@Type", goalType.ToString());
			cmd.Parameters.AddWithValue("@Start", startTime.ToString());
			cmd.Parameters.AddWithValue("@End", endTime.ToString());
			cmd.Parameters.AddWithValue("@GoalStartAmount", goalType == GoalType.Lines ? startLines : startProgrammingTime);
			cmd.Parameters.AddWithValue("@GoalAmountLeft", goalType == GoalType.Lines ? linesLeft : programmingTimeLeft);
			cmd.ExecuteNonQuery();
		}
    }

    public string[] SetTableRow(int index)
    {
        string[] row = new string[5];

        row[0] = (index + 1).ToString();
		row[1] = goalType == GoalType.Lines ? "To produce a certain amount of coding lines" : "To program for a certain amount of time";
        row[2] = "In progress";
        row[3] = TimeSpanView((endTime - DateTime.Now));
        row[4] = goalType == GoalType.Lines ? linesLeft.ToString() + " lines" : TimeSpanView(programmingTimeLeft);
        return row;
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
