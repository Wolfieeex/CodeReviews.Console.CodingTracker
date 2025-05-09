using Console.CodingTracker.Controller.ScreenMangers;
using Console.CodingTracker.Model;
using Console.CodingTracker.View;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.Controller.CRUD;

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

internal class Reporting
{
    internal static void GenerateReport()
    {
        Color titleColor = Color.Purple;
        Color mainColor = Color.Purple_1;
        Color inputColor = Color.SlateBlue1;

        string titleColorHex = "[#" + titleColor.ToHex() + "]";
        string inputColorHex = "[#" + inputColor.ToHex() + "]";

		ReportSettings reportSettings = TemporaryData.ReportConfiguration;
        FilterDetails filterDetails = TemporaryData.LastFilter;
        reportSettings.FilterDetails = filterDetails;

        string? reportOptionsString = null;
        string? dataOptionsString = null;
        string? PeriodSelectionString = null;

        bool loopReportMenu = true;
        while (loopReportMenu)
        {
			System.Console.Clear();

			bool wasFilterSelected = true;
            if (filterDetails.FromDate == null && filterDetails.ToDate == null && filterDetails.MinLines == null && filterDetails.MaxLines == null && filterDetails.Comment == null && filterDetails.MinDuration == null && filterDetails.MaxDuration == null && filterDetails.WasTimerTracked == null)
            {
                wasFilterSelected = false;
            }
            try
            {
                if (Enum.GetNames(typeof(ReportOptions)).Length != reportSettings.ReportOptions.Length)
                {
                    throw new DataMisalignedException("ReportOptions Enum must have the same length as ReportConfiguration.ReportOptions array length.");
                }
                if (Enum.GetNames(typeof(SummationOptions)).Length != reportSettings.DataOptions.Length)
                {
                    throw new DataMisalignedException("SummationOptions Enum must have the same length as ReportConfiguration.DataOptions array length.");
                }

                if (reportSettings.ReportOptions != null)
                {
                    reportOptionsString = "";
                    int counter = 0;
                    foreach (string s in Enum.GetNames(typeof(ReportOptions)))
                    {
                        if (reportSettings.ReportOptions[counter])
                        {
                            reportOptionsString += Regex.Replace(s, @"(?<=[a-z])([A-Z]{1})", " $1");
                            reportOptionsString += ", ";
                        }
                        counter++;
                    }
                    reportOptionsString = reportOptionsString.Remove(reportOptionsString.Length - 2, 2);
                }
                else
                {
                    reportOptionsString = null;
                }
                if (reportSettings.DataOptions != null)
                {
                    dataOptionsString = "";
                    int counter = 0;
                    foreach (string s in Enum.GetNames(typeof(SummationOptions)))
                    {
                        if (reportSettings.DataOptions[counter])
                        {
                            dataOptionsString += Regex.Replace(s, @"(?<=[a-z])([A-Z]{1})", " $1");
                            dataOptionsString += ", ";
                        }
                        counter++;
                    }
                    dataOptionsString = dataOptionsString.Remove(dataOptionsString.Length - 2, 2);
                }
                else
                {
                    dataOptionsString = null;
                }
                if (reportSettings.Period != null)
                {
                    PeriodSelectionString = "";
                    PeriodSelectionString += Enum.GetName(typeof(ReportSortationPeriod), reportSettings.Period);

                    if (reportSettings.SortationYear != null)
                    {
                        PeriodSelectionString += ", " + reportSettings.SortationYear.ToString();
                    }
                    if (reportSettings.SortationMonth != null)
                    {
                        PeriodSelectionString += ", " + Enum.GetName(typeof(Months), reportSettings.SortationMonth);
                    }
                }
            }
            catch
            {
                System.Console.ReadKey();
            }

            Dictionary<string, string> reportMenuDic = new Dictionary<string, string>()
            {
                { Enum.GetName((ReportMenu)0), wasFilterSelected ? "Filter(s) selected" : "No filters"},
                { Enum.GetName((ReportMenu)1), reportOptionsString },
                { Enum.GetName((ReportMenu)2), dataOptionsString },
                { Enum.GetName((ReportMenu)3), PeriodSelectionString }
            };

            int? userInput = UserInterface.DisplaySelectionUIWithUserInputs($"\nYou are currently in the {titleColorHex}report generation menu[/]. ", typeof(ReportMenu), titleColor, mainColor, inputColor, reportMenuDic, $"{Settings.OptionalsCompleted}Run report[/]", false);

            bool[] tempOptions;
            switch (userInput)
            {
                case -1:
                    reportSettings.FilterDetails = filterDetails;
                    TemporaryData.ReportConfiguration = reportSettings;
                    TemporaryData.LastFilter = filterDetails;

                    Dictionary<string, List<string>> DurationTable = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> LinesTable = new Dictionary<string, List<string>>();
                    bool noRecords = SQL.Reporting.CalculateReport(reportSettings, out DurationTable, out LinesTable, titleColor);
                    if (!noRecords || DurationTable == null || LinesTable == null || DurationTable.Count == 0)
                    {
                        LinesTable ??= new Dictionary<string, List<string>>();
                        Tables.DrawReportTable(reportSettings, DurationTable, LinesTable, titleColorHex, inputColorHex);
					}
                    break;
                case 0:
                    bool runFilterMenu = true;
                    while (runFilterMenu)
                    {
                        Dictionary<string, string> filterDic = new Dictionary<string, string>()
                        {
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)1), filterDetails.FromDate},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)2), filterDetails.ToDate},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)3), filterDetails.MinLines},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)4), filterDetails.MaxLines},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)5), filterDetails.Comment},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)6), filterDetails.MinDuration},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)7), filterDetails.MaxDuration},
                            { Enum.GetName(typeof(MenuSelections.FilterRecordsForReport), (MenuSelections.FilterRecordsForReport)8), filterDetails.WasTimerTracked}
                        };
                        System.Console.Clear();
                        bool shouldBlock = false;
                        string reason = "";
                        FilterController.CheckFilterConditions(filterDetails, ref reason, ref shouldBlock, mainColor);
                        FilterScreenManager.ReportFilterMenu($"Records used to calculate your report will be {titleColorHex}selected by your filters. [/]", ref filterDetails, ref runFilterMenu, filterDic, reason, shouldBlock, titleColor, mainColor, inputColor);
                    }
                    break;
                case 1:
                    tempOptions = reportSettings.ReportOptions;
                    UserInterface.DisplayMultiselectionUI($"Select {titleColorHex}data to display for your report[/]:", typeof(ReportOptions), ref tempOptions, mainColor);
                    reportSettings.ReportOptions = tempOptions;
                    break;
                case 2:
                    tempOptions = reportSettings.DataOptions;
                    UserInterface.DisplayMultiselectionUI($"Select {titleColorHex}variables by which your report will calculated[/]:", typeof(SummationOptions), ref tempOptions, mainColor );
                    reportSettings.DataOptions = tempOptions;
                    break;
                case 3:
                    PeriodSelectionMenu(ref reportSettings);
                    break;
                case 4:
                    loopReportMenu = false;
                    TemporaryData.ReportConfiguration = reportSettings;
                    TemporaryData.LastFilter = filterDetails;
                    break;
            }
        }
    }
    internal static void PeriodSelectionMenu(ref ReportSettings reportSettings)
    {
        bool runPeriodMenu = true;
        while (runPeriodMenu)
        {
            List<string> years = SQL.Reporting.ReturnAllRecordedYears();

            var enumReturn = UserInterface.DisplayEnumSelectionUI("Select by [purple]which period you would like to generate your report:[/]", typeof(ReportSortationPeriod), Color.Purple4);
            if (enumReturn is not int)
            {
                if (enumReturn == ReportSortationPeriod.Yearly)
                {
                    reportSettings.Period = enumReturn;
                    reportSettings.SortationYear = null;
                    reportSettings.SortationMonth = null;
                    runPeriodMenu = false;
                    break;
                }

                bool runYearSelectionMenu = true;
                while (runYearSelectionMenu)
                {
                    string year = UserInterface.DisplayStringListSelectionUI("Select [purple]by which year[/] the report should be built:", years, Color.Purple3);
                    if (year == "0")
                    {
                        runYearSelectionMenu = false;
                        continue;
                    }
                    if (year == "-1")
                    {
                        runYearSelectionMenu = false;
                        runPeriodMenu = false;
                        break;
                    }
                    if (enumReturn == ReportSortationPeriod.Monthly || enumReturn == ReportSortationPeriod.Weekly)
                    {
                        reportSettings.Period = enumReturn;
                        reportSettings.SortationYear = int.Parse(year);
                        reportSettings.SortationMonth = null;
                        runPeriodMenu = false;
                        break;
                    }

                    bool runMonthSelectionMenu = true;
                    while (runMonthSelectionMenu)
                    {
                        List<int> months = SQL.Reporting.ReturnRecordedMonthsForYear(int.Parse(year));
                        for (int i = 0; i < months.Count; i++)
                        {
                            months[i] -= 1;
                        }
                        List<Months> enumMonths = new List<Months>();
                        for (int i = 0; i < months.Count; i++)
                        {
                            enumMonths.Add((Months)months[i]);
                        }

                        string monthSelection = UserInterface.DisplayStringListSelectionUI("Select [purple]by which month[/] the report should be built:", enumMonths.Select(s => Enum.GetName(typeof(Months), s)).ToList(), Color.Purple3);
                        if (monthSelection == "0")
                        {
                            runMonthSelectionMenu = false;
                            continue;
                        }

                        runYearSelectionMenu = false;
                        runMonthSelectionMenu = false;
                        runPeriodMenu = false;

                        if (monthSelection == "-1")
                        {
                            break;
                        }
                        reportSettings.Period = enumReturn;
                        reportSettings.SortationYear = int.Parse(year);
                        reportSettings.SortationMonth = (Months)Enum.Parse(typeof(Months), monthSelection);
                        runPeriodMenu = false;
                    }
                }
            }
            else
            {
                runPeriodMenu = false;
            }
        }

    }
}
