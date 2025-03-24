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
    private TimeSpan time;
    private TimeSpan timeToComplete;
    private int lines;

    /// <summary>
    /// Creates a goal with time of programming span type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(TimeSpan time, TimeSpan timeToComplete)
    {
        this.time = time;
        this.timeToComplete = timeToComplete;
        goalType = GoalType.Time;
    }

    /// <summary>
    /// Creates a goal with number of lines produced type, the 2nd parameter accepts the time in which goal needs to be completed.
    /// </summary>
    public Goal(int lines, TimeSpan timeToComplete)
    {
        this.lines = lines;
        this.timeToComplete = timeToComplete;
        goalType = GoalType.Lines;
    }
}
