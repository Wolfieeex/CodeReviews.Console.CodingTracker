using static CodingTracker.Wolfieeex.Model.ReportingEnums;

namespace CodingTracker.Wolfieeex.Model;

struct ReportCalculationsToDisplay
{
    public bool CountRecords;
    public bool SumRecords;
    public bool FindMaximalValues;
    public bool FindMinimalValues;
    public bool FindMeanValues;
    public bool FindMedianValues;
    public bool FindModalValues;
    public bool IsInitialized = false;

    public ReportCalculationsToDisplay(bool[] values)
    {
        if (values == null || values.Length != 7)
            throw new ArgumentException("Array of bool sent to create struct ReportCalculationsToDisplay must contain exactly 7 boolean values in its array.");

        CountRecords = values[0];
        SumRecords = values[1];
        FindMaximalValues = values[2];
        FindMinimalValues = values[3];
        FindMeanValues = values[4];
        FindMedianValues = values[5];
        FindModalValues = values[6];
        IsInitialized = true;
    }

    public bool[] InfoToBoolArray()
    {
        return new[]
        {
            CountRecords,
            SumRecords,
            FindMaximalValues,
            FindMinimalValues,
            FindMeanValues,
            FindMedianValues,
            FindModalValues
        };
    }
}

struct TypeOfDataChosenToCalculateReport
{
    public bool Duration;
    public bool Lines;
    public bool IsInitialized = false;

    public TypeOfDataChosenToCalculateReport(bool[] values)
    {
        if (values == null || values.Length != 2)
            throw new ArgumentException("Array of bool sent to create struct ReportCalculationsToDisplay must contain exactly 2 boolean values in its array.");

        Duration = values[0];
        Lines = values[1];
        IsInitialized = true;
    }

    public bool[] InfoToBoolArray()
    {
        return new[]
        {
            Duration,
            Lines,
        };
    }
}

internal class ReportSettings
{
    internal FilterDetails FilterDetails { get; set; }
    internal ReportCalculationsToDisplay ReportOptions { get; set; }
    internal TypeOfDataChosenToCalculateReport DataOptions { get; set; }
    internal ReportSortationPeriod Period { get; set; }
    internal int? SortationYear { get; set; }
    internal List<string>? YearWeeks { get; set; }
    internal Months? SortationMonth { get; set; }
}
