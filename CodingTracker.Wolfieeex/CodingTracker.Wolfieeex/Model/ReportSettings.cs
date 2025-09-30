using static CodingTracker.Wolfieeex.Model.ReportingEnums;

namespace CodingTracker.Wolfieeex.Model;

// Create a struct for both ReportOptions and DataOptions. Explain more in detail using correct name the rest of the fields.
internal class ReportSettings
{
    internal FilterDetails FilterDetails { get; set; }
    internal bool[] ReportOptions { get; set; }
    internal bool[] DataOptions { get; set; }
    internal ReportSortationPeriod Period { get; set; }
    internal int? SortationYear { get; set; }
    internal List<string>? YearWeeks { get; set; }
    internal Months? SortationMonth { get; set; }
}
