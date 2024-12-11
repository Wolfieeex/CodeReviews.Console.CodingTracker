namespace Console.CodingTracker.Model;

internal class Session
{
    internal string CreationDate { get; private set; }
    internal string LastUpdateData { get; private set; }
    internal string StartDate { get; private set; }
    internal string EndDate { get; private set; }
    internal string Duration { get; private set; }
    internal int? NumberOfLines { get; private set; }
    internal string? Comments { get; private set; }
    internal bool WasTimerTracked { get; private set; }

    public Session(string start, string end, int? lines, string? comments, bool wasTimerTracked = false)
    {
        StartDate = start;
        EndDate = end;
        NumberOfLines = lines;
        Comments = comments;
        WasTimerTracked = wasTimerTracked;
    }

    public Session(string creation, string lastUpdate, string start, string end, string duration, int? lines, string? comments, bool wasTimerTracked = false)
    {
        CreationDate = creation;
        LastUpdateData = lastUpdate;
        StartDate = start;
        EndDate = end;
        Duration = duration;
        NumberOfLines = lines;
        Comments = comments;
        WasTimerTracked = wasTimerTracked;
    }
}
