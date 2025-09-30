namespace Console.CodingTracker.Model;

public struct ViewOptions
{
    bool CreationDate;
    bool LastUpdateWasOn;
    bool StartDatetime;
    bool EndDateTime;
    bool Duration;
    bool NumberOfLines;
    bool Comments;
    bool WasSessionTrackedByTimer;
}

internal class FilterDetails
{
    public SortingDetails SortingDetails { get; set; }
    public ViewOptions ViewOptions { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public string? MinLines { get; set; }
    public string? MaxLines { get; set; }
    public string? Comment { get; set; }
    public string? MinDuration { get; set; }
    public string? MaxDuration { get; set; }
    public string? WasTimerTracked { get; set; }
}

internal class SortingDetails
{
    public Controller.CRUD.SortingBy? SortBy { get; set; }
    public Controller.CRUD.SortingOrder? SortOrder { get; set; }
}