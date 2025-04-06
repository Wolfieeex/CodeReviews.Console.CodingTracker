using Microsoft.Data.Sqlite;
using System.Configuration;

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
	public GoalType GoalType { get; init; }
	public DateTime StartTime { get; init; }
	public DateTime EndTime { get; init; }

	public TimeSpan StartProgrammingTime { get; init; }
	private TimeSpan _programmingTimeLeft;
	public TimeSpan ProgrammingTimeLeft { get => _programmingTimeLeft; set {
            if (value <= TimeSpan.Zero)
				_programmingTimeLeft = TimeSpan.Zero;
            else
				_programmingTimeLeft = value; } }
	
    public int StartLines { get; init; }
	private int _linesLeft;
	public int LinesLeft { get => _linesLeft; set
        {
            if (_linesLeft <= 0)
				_linesLeft = 0;
            else
				_linesLeft = value;
        } }
    
    private string Status { get
        {
            if (EndTime - DateTime.Now < TimeSpan.Zero)
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
            return "InProgress";
        ;} }

    /// <summary>
    /// Creates a goal with time of programming span type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(DateTime startTime, TimeSpan timeToComplete, TimeSpan startProgrammingTime, TimeSpan programmingTimeLeft)
    {
        this.StartTime = startTime;
		EndTime = startTime + timeToComplete;
		this.StartProgrammingTime = startProgrammingTime;
		this.ProgrammingTimeLeft = programmingTimeLeft;
		GoalType = GoalType.Time;
    }

    /// <summary>
    /// Creates a goal with number of lines produced type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(DateTime startTime, TimeSpan timeToComplete, int startLines, int linesLeft)
    {
		this.StartTime = startTime;
		EndTime = startTime + timeToComplete;
        this.StartLines = startLines;
		this.LinesLeft = linesLeft;
		GoalType = GoalType.Lines;
    }
	public void AddGoalToDatabase()
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO {ConfigurationManager.AppSettings.Get("GoalDatabaseName")}(Goal, Status, [Start Date], [End Date], [Start Goal Amount], [Goal Amount Left]) VALUES (@Type, 'InProgress', @Start, @End, @GoalStartAmount, @GoalAmountLeft)";
			cmd.Parameters.AddWithValue("@Type", GoalType.ToString());
			cmd.Parameters.AddWithValue("@Start", StartTime.ToString());
			cmd.Parameters.AddWithValue("@End", EndTime.ToString());
			cmd.Parameters.AddWithValue("@GoalStartAmount", GoalType == GoalType.Lines ? StartLines : StartProgrammingTime);
			cmd.Parameters.AddWithValue("@GoalAmountLeft", GoalType == GoalType.Lines ? LinesLeft : ProgrammingTimeLeft);
			cmd.ExecuteNonQuery();
		}
    }
    public string[] SetTableRow(int index)
    {
        string[] row = new string[5];

        row[0] = (index + 1).ToString();
		row[1] = GoalType == GoalType.Lines ? "To produce a certain amount of coding lines" : "To program for a certain amount of time";
        row[2] = Status;
        row[3] = TimeSpanView((EndTime - DateTime.Now));
        row[4] = GoalType == GoalType.Lines ? LinesLeft.ToString() + " lines" : TimeSpanView(ProgrammingTimeLeft);
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
