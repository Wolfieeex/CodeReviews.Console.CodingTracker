namespace CodingTracker.Wolfieeex.Model;

internal class CodingSession
{
    public CodingSession() { }
    public CodingSession(string creation, string lastUpdate, string start, string end, string duration, int? lines, string? comments, string wasTimerTracked = "false")
    {
        CreationDate = creation;
        LastUpdateDate = lastUpdate;
        StartDate = start;
        EndDate = end;
        Duration = duration;
        LinesOfCode = lines;
        Comments = comments;
        WasTimerTracked = wasTimerTracked;
    }
    public CodingSession(int key, string creation, string lastUpdate, string start, string end, string duration, int? lines, string? comments, string wasTimerTracked = "false")
    {
        Id = key;
        CreationDate = creation;
        LastUpdateDate = lastUpdate;
        StartDate = start;
        EndDate = end;
        Duration = duration;
        LinesOfCode = lines;
        Comments = comments;
        WasTimerTracked = wasTimerTracked;
    }
    internal int Id { get; set; }
    internal string CreationDate { get; set; }
    internal string LastUpdateDate { get; set; }
    internal string StartDate { get; set; }
    internal string EndDate { get; set; }
    internal string Duration { get; set; }
    internal int? LinesOfCode { get; set; }
    internal string? Comments { get; set; }
    internal string WasTimerTracked { get; set; }
}
