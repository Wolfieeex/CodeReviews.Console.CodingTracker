namespace Console.CodingTracker.Model;

internal class FilterDetails
{
    public SortingDetails SortingDetails { get; set; }
    public bool[] ViewOptions { get; set; }
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
    public MenuSelections.SortingBy? SortBy { get; set; }
    public MenuSelections.SortingOrder? SortOrder { get; set; }
}