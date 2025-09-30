namespace CodingTracker.Wolfieeex.Model;

internal class ReportingEnums
{
	public enum SortingOrder
	{
		Ascending,
		Descending
	}

	internal enum SortingBy
	{
		CreationDate,
		UpdateDate,
		StartDate,
		EndDate,
		Duration,
		NumberOfLines,
		Comment,
		WasTimerTracked
	}

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
}
