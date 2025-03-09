using Console.CodingTracker.Controller.ScreenMangers;
using Console.CodingTracker.Model;
using Console.CodingTracker.View;
using Spectre.Console;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.Controller.CRUD;
internal class FilterController
{
    internal static FilterDetails FilterRecords(string preTitle, ref bool returnToMenu, Color titleColor, Color mainColor, Color inputColor)
    {
        FilterDetails filterDetails = new FilterDetails()
        {
            SortingDetails = TemporaryData.lastFilter.SortingDetails,
            ViewOptions = TemporaryData.lastFilter.ViewOptions,
            FromDate = TemporaryData.lastFilter.FromDate,
            ToDate = TemporaryData.lastFilter.ToDate,
            MinLines = TemporaryData.lastFilter.MinLines,
            MaxLines = TemporaryData.lastFilter.MaxLines,
            Comment = TemporaryData.lastFilter.Comment,
            MinDuration = TemporaryData.lastFilter.MinDuration,
            MaxDuration = TemporaryData.lastFilter.MaxDuration,
            WasTimerTracked = TemporaryData.lastFilter.WasTimerTracked
        };
        SortingDetails sortingDetails = TemporaryData.lastFilter.SortingDetails;

        bool runFilterMenuLoop = true;
        while (runFilterMenuLoop)
        {
            string sortingDetailsString = sortingDetails.SortBy == null || sortingDetails.SortOrder == null ? null : sortingDetails.SortOrder.ToString() + ", " + Regex.Replace(sortingDetails.SortBy.ToString(), @"(?<=[A-Za-z])([A-Z])", @" $1");
            Dictionary<string, string> dic = new Dictionary<string, string>()
        {
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)2), sortingDetailsString},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)3), filterDetails.FromDate},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)4), filterDetails.ToDate},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)5), filterDetails.MinLines},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)6), filterDetails.MaxLines},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)7), filterDetails.Comment},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)8), filterDetails.MinDuration},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)9), filterDetails.MaxDuration},
            { Enum.GetName(typeof(MenuSelections.FilterRecords), (MenuSelections.FilterRecords)10), filterDetails.WasTimerTracked}
        };

            System.Console.Clear();

            string reason = "";
            bool shouldBlock = false;
            CheckFilterConditions(filterDetails, ref reason, ref shouldBlock);

            FilterScreenManager.BasicFilterMenu(preTitle, ref returnToMenu, ref filterDetails, ref sortingDetails, ref runFilterMenuLoop, dic, reason, shouldBlock, titleColor, mainColor, inputColor);
        }
        return filterDetails;
    }
    internal static SortingDetails SortingMenu(SortingDetails previousDetails, Color titleColor, Color mainColor, Color inputColor)
    {
        bool inSortingMenu = true;
        SortingDetails sortingDetails = TemporaryData.lastFilter.SortingDetails;

        while (inSortingMenu)
        {
            Dictionary<string, string> filteringSelections = new Dictionary<string, string>()
            {
                { Enum.GetName(typeof(MenuSelections.SortingMenu), (MenuSelections.SortingMenu)1), sortingDetails.SortOrder == null ? null : Regex.Replace(Enum.GetName(sortingDetails.SortOrder.GetType(), sortingDetails.SortOrder), @"(?<=[A-Za-z])([A-Z])", @" $1")},
                { Enum.GetName(typeof(MenuSelections.SortingMenu), (MenuSelections.SortingMenu)2), sortingDetails.SortBy == null ? null : Regex.Replace(Enum.GetName(sortingDetails.SortBy.GetType(), sortingDetails.SortBy), @"(?<=[A-Za-z])([A-Z])", @" $1")},
            };
            int? userSelection = UserInterface.DisplaySelectionUIWithUserInputs($"Please [{titleColor}]select your sorting options:[/]", typeof(MenuSelections.SortingMenu), titleColor, mainColor, inputColor, filteringSelections, "[green]Execute[/]", false);

            switch (userSelection)
            {
                case -1:
                    TemporaryData.lastFilter.SortingDetails = sortingDetails;
                    return sortingDetails;
                case 0:
                    sortingDetails.SortBy = null;
                    sortingDetails.SortOrder = null;
                    break;
                case 1:
                    dynamic resultSortingOrder = UserInterface.DisplayEnumSelectionUI("Please select your sorting order: ", typeof(SortingOrder), Color.LightSeaGreen);
                    if (!(resultSortingOrder is int))
                    {
                        sortingDetails.SortOrder = resultSortingOrder;
                    }
                    break;
                case 2:
                    dynamic resultSortingBy = UserInterface.DisplayEnumSelectionUI("Please select your sorting option: ", typeof(SortingBy), Color.LightSeaGreen);
                    if (!(resultSortingBy is int))
                    {
                        sortingDetails.SortBy = resultSortingBy;
                    }
                    break;
                case 3:
                    return new SortingDetails()
                    {
                        SortBy = null,
                        SortOrder = null
                    };
            }
        }
        return null;
    }
    public static void CheckFilterConditions(FilterDetails filterDetails, ref string reason, ref bool shouldBlock)
    {
        if (!string.IsNullOrEmpty(filterDetails.FromDate) && !string.IsNullOrEmpty(filterDetails.ToDate))
        {
            DateTime dateStart = DateTime.Parse(filterDetails.FromDate);
            DateTime dateEnd = DateTime.Parse(filterDetails.ToDate);

            if (dateEnd < dateStart)
            {
                reason += "[red]The start date of your session must be before the end date of your session.[/]\n";
                shouldBlock = true;
            }
        }
        if (!string.IsNullOrEmpty(filterDetails.MaxLines) && !string.IsNullOrEmpty(filterDetails.MinLines))
        {

            if (int.Parse(filterDetails.MaxLines) < int.Parse(filterDetails.MinLines))
            {
                reason += "[red]Minimal number of lines cannot exceed maximal lines search.[/]\n";
                shouldBlock = true;
            }
        }
        if (!string.IsNullOrEmpty(filterDetails.MaxDuration) && !string.IsNullOrEmpty(filterDetails.MinDuration))
        {
            if (TimeSpan.ParseExact(filterDetails.MaxDuration, @"d\ hh\:mm", new CultureInfo("en-GB"), TimeSpanStyles.None) < TimeSpan.ParseExact(filterDetails.MinDuration, @"d\ hh\:mm", new CultureInfo("en-GB"), TimeSpanStyles.None))
            {
                reason += "[red]Your maximal session time needs to be longer than the minimal session time.[/]\n";
                shouldBlock = true;
            }
        }
    }
}
