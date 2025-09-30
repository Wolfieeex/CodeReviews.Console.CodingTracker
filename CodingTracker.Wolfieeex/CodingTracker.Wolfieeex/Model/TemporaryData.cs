using static CodingTracker.Wolfieeex.Model.ReportingEnums;

namespace CodingTracker.Wolfieeex.Model;

internal class TemporaryData
{
    internal static FilterDetails LastFilter { get; set; }
    // That's next :)
    internal static ReportSettings ReportConfiguration { get; set; } = new ReportSettings()
    {
        ReportOptions = new bool[] { true, false, false, false, false, false, false },
        DataOptions = new bool[] { true, false }
    };

	internal static void SetFilterSettingsToDefaultSettings()
	{
		LastFilter = new FilterDetails()
		{
			ViewOptions = new FieldsToViewOnDataTable(new bool[] { false, false, false, false, true, true, true, false }),
			SortingDetails = new SortingDetails()
			{
				SortBy = null,
				SortOrder = ReportingEnums.SortingOrder.Ascending
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
}
