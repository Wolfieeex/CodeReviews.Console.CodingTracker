using Console.CodingTracker.MenuSelections;

namespace Console.CodingTracker.Model;

internal class ReportSettings
{
    internal ReportSortationPeriod Period { get; set; }
    internal int? SortationYear { get; set; }
    internal Months? Month { get; set; }
    internal bool[] ReportOptions { get; set; }
    internal FilterDetails FilterDetails { get; set; }
    internal bool[] DataOptions { get; set; }
}
