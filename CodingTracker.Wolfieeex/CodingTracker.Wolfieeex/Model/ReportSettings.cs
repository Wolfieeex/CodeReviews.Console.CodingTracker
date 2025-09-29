namespace Console.CodingTracker.Model;
internal enum ReportMenu
{
    OptionalApplyFilters,
    ReportOptions,
    DataOptions,
    PeriodSelection,
    ReturnToMainMenu
}
internal enum SummationOptions
{
    Durations,
    Lines
}
internal enum ReportOptions
{
    CountRecords,
    SumRecords,
    FindMaximalValues,
    FindMinimalValues,
    FindMeanValues,
    FindMedianValues,
    FindModalValues
}
internal enum ReportSortationPeriod
{
    Yearly,
    Monthly,
    Weekly,
    Daily
}
internal enum Months
{
    January,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    December
}
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
