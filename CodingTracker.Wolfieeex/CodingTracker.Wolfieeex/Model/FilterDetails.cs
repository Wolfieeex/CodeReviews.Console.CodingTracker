namespace CodingTracker.Wolfieeex.Model;

struct FieldsToViewOnDataTable
{
    public bool CreationDate;
	public bool LastUpdateWasOn;
	public bool StartDatetime;
	public bool EndDateTime;
	public bool Duration;
	public bool NumberOfLines;
	public bool Comments;
	public bool WasSessionTrackedByTimer;
	public bool IsInitialized = false;

	public FieldsToViewOnDataTable(bool[] values)
	{
		if (values.Length != 8)
			throw new ArgumentException("Expected 8 values while setting up FieldsToViewOnDataTable for FilterDetails class.");

		CreationDate = values[0];
		LastUpdateWasOn = values[1];
		StartDatetime = values[2];
		EndDateTime = values[3];
		Duration = values[4];
		NumberOfLines = values[5];
		Comments = values[6];
		WasSessionTrackedByTimer = values[7];
		IsInitialized = true;
	}

	public bool[] InfoToBoolArray()
	{
		return new[]
		{
			CreationDate,
			LastUpdateWasOn,
			StartDatetime,
			EndDateTime,
			Duration,
			NumberOfLines,
			Comments,
			WasSessionTrackedByTimer
		};
	}
}

internal class FilterDetails
{
	public SortingDetails SortingDetails { get; set; }
	public FieldsToViewOnDataTable ViewOptions { get; set; }
	public string? FromDate { get; set; }
	public string? ToDate { get; set; }
	public string? MinLines { get; set; }
	public string? MaxLines { get; set; }
	public string? Comment { get; set; }
	public string? MinDuration { get; set; }
	public string? MaxDuration { get; set; }
	public string? WasTimerTracked { get; set; }

	public FilterDetails()
	{
		ViewOptions = new FieldsToViewOnDataTable(new bool[] { false, false, false, false, true, true, true, false });
		SortingDetails = new SortingDetails()
		{
			SortBy = null,
			SortOrder = ReportingEnums.SortingOrder.Ascending
		};
		FromDate = null;
		ToDate = null;
		MinLines = null;
		MaxLines = null;
		Comment = null;
		MinDuration = null;
		MaxDuration = null;
		WasTimerTracked = null;
	}
}