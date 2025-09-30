using System.Xml.Linq;

namespace Console.CodingTracker.Model;

internal class TemporaryData
{
    internal static FilterDetails LastFilter { get; set; }
    internal static void SetFilterSettingsToDefaultSettings()
    {
        LastFilter = new FilterDetails()
        {
            ViewOptions = new bool[] { false, false, false, false, true, true, true, false },
            SortingDetails = new SortingDetails()
            {
                SortBy = null,
                SortOrder = Controller.CRUD.SortingOrder.Ascending
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
    internal static ReportSettings ReportConfiguration { get; set; } = new ReportSettings()
    {
        ReportOptions = new bool[] { true, false, false, false, false, false, false },
        DataOptions = new bool[] { true, false }
    };
}
