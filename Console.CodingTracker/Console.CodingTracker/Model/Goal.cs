using Microsoft.Data.Sqlite;

namespace Console.CodingTracker.Model;

public enum GoalStatus
{
    Completed,
    InProgress,
    Failed,
    Deleted
}

public enum GoalType
{
    Time,
    Lines
}

internal class Goal
{
    private GoalType goalType;
    private string time;
    private string timeToComplete;
    private int lines;

    /// <summary>
    /// Creates a goal with time of programming span type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(string time, string timeToComplete)
    {
        this.time = time;
        this.timeToComplete = timeToComplete;
        goalType = GoalType.Time;
    }

    /// <summary>
    /// Creates a goal with number of lines produced type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(int lines, string timeToComplete)
    {
        this.lines = lines;
        this.timeToComplete = timeToComplete;
        goalType = GoalType.Lines;
    }

    public void AddGoalToDatabase()
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO {Settings.GoalDatabaseName}(Goal, Status, [Goal amount left], [Time left]) VALUES (@Type, 'In Progress', @Goal, @TimeToComplete)";
			cmd.Parameters.AddWithValue("@Type", nameof(goalType));
			cmd.Parameters.AddWithValue("@Goal", goalType == GoalType.Lines ? lines.ToString() : time);
			cmd.Parameters.AddWithValue("@TimeToComplete", timeToComplete);
            cmd.ExecuteNonQuery();
		}
    }
}
