using Console.CodingTracker.MenuSelections;

namespace Console.CodingTracker.Model;

internal class ReportSettings
{
    internal ReportSortationPeriod Period { get; set; }
    internal bool[] ReportTableOptions { get; set; }
    internal FilterDetails FilterDetails { get; set; }
    internal bool[] ReportSummationOptions { get; set; }
}
