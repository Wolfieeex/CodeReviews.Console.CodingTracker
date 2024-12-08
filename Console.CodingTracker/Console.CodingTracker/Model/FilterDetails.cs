namespace Console.CodingTracker.Model;

internal class FilterDetails
{
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public string? MinLines { get; set; }
    public string? MaxLines { get; set; }
    public string? Comment { get; set; }
    public string? MinDuration { get; set; }
    public string? MaxDuration { get; set; }
}