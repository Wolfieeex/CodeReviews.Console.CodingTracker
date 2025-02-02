using System.Xml.Linq;

namespace Console.CodingTracker.Model;

internal class TemporaryData
{
    internal static FilterDetails lastFilter { get; set; }
    internal static void InitializeLastFilter()
    {
        lastFilter = new FilterDetails()
        {
            ViewOptions = new bool[] { false, false, false, false, true, true, true, false },
            SortingDetails = new SortingDetails()
            {
                SortBy = null,
                SortOrder = MenuSelections.SortingOrder.Ascending
            },
            FromDate = null,
            ToDate = null,
            MinLines = null,
            MaxLines = null,
            Comment = null,
            MinDuration = null,
            MaxDuration = null,
            WasTimerTracked = null
        };
    }
    internal static ReportSettings reportSettings { get; set; } = new ReportSettings()
    {
        ReportTableOptions = new bool[] { true, false, false, false, false, false, false },
        ReportSummationOptions = new bool[] { true, false }
    };
}
