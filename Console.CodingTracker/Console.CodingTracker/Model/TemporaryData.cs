using System.Xml.Linq;

namespace Console.CodingTracker.Model;

internal class TemporaryData
{
    internal static FilterDetails lastFilter { get; set; }

    internal static void InitializeLastFilter()
    {
        lastFilter = new FilterDetails()
        {
            FromDate = null,
            ToDate = null,
            MinLines = null,
            MaxLines = null,
            Comment = null,
            MinDuration = null,
            MaxDuration = null
        };
    }
}
