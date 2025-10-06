namespace CodingTracker.Wolfieeex.Model;

internal static class TemporaryData
{
	internal static FilterDetails LastFilter { get; set; } = new();
    
    internal static ReportSettings ReportConfiguration { get; set; } = new ReportSettings()
    {
        ReportOptions = new ReportCalculationsToDisplay(new bool[] { true, false, false, false, false, false, false }),
        DataOptions = new TypeOfDataChosenToCalculateReport(new bool[] { true, false })
    };
}
