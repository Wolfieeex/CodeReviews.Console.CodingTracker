using Console.CodingTracker.Model;
using Dapper;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.Controller.SQL
{
    internal class Reporting
    {
        public static bool CalculateReport(ReportSettings settings, out Dictionary<string, List<string>> DurationTable, out Dictionary<string, List<string>> LinesTable, Color titleColor)
        {
            List<CodingSession> sessions = Crud.GetRecords(settings.FilterDetails);
            if (sessions.Count == 0)
            {
                System.Console.Clear();
                AnsiConsole.MarkupLine($"Cannot generate report with selected settings- no records found. [#{titleColor.Blend(Color.Red, 0.3f).ToHex()}]Please select DIFFERENT FILTERS and try again.[/]");
                DurationTable = null;
                LinesTable = null;
                return true;
            }
            else
            {
                Dictionary<string, List<string>> durationData = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> linesData = new Dictionary<string, List<string>>();

                string whereFilterInject = "";
                string whereReportInject = "";
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                whereFilterInject = Filtering.AddQueryFilterParameters(settings.FilterDetails, whereFilterInject, parameters);

                List<string> periods = new List<string>();
                List<Tuple<string, string>> weeksPeriods = new List<Tuple<string, string>>();
                switch (settings.Period)
                {
                    case ReportSortationPeriod.Daily:
                        int daysInMonth = DateTime.DaysInMonth(settings.SortationYear.Value, (int)settings.SortationMonth + 1);
                        for (int i = 1; i <= daysInMonth; i++)
                        {
                            periods.Add(i.ToString("00"));
                        }
                        break;
                    case ReportSortationPeriod.Weekly:
                        weeksPeriods = ReturnAllWeeksOfTheYear(settings.SortationYear.Value);
                        foreach (Tuple<string, string> wp in weeksPeriods)
                        {
                            periods.Add(wp.Item1 + wp.Item2);
                        }
                        break;
                    case ReportSortationPeriod.Monthly:
                        periods = ReturnRecordedMonthsForYear(settings.SortationYear != null ? settings.SortationYear.Value : 0).Select(x => x.ToString("00")).ToList();
                        break;
                    case ReportSortationPeriod.Yearly:
                        periods = ReturnAllRecordedYears();
                        break;
                }

                using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
                {
                    conn.Open();
                    List<CodingSession> records = new List<CodingSession>();

                    foreach (string per in periods)
                    {
                        records.Clear();
                        durationData.Add(per, new List<string>());
                        linesData.Add(per, new List<string>());
                        switch (settings.Period)
                        {
                            case ReportSortationPeriod.Daily:
                                whereReportInject = $@"substr(""End date"", 1, 2) == '{per}'";
                                break;
                            case ReportSortationPeriod.Weekly:
                                string sqlDateTranslatiion = $@"substr(""End date"", 7, 4) || '-' || substr(""End date"", 4, 2) || '-' || substr(""End date"", 1, 2)";
                                string startDate = per.Substring(per.IndexOf('.') + 1, 10);
                                string endDate = per.Substring(per.IndexOf('.') + 11, 10);
                                whereReportInject = $@"{sqlDateTranslatiion} >= '{startDate}' AND {sqlDateTranslatiion} <= '{endDate}'";
                                break;
                            case ReportSortationPeriod.Monthly:
                                whereReportInject = $@"substr(""End date"", 4, 2) == '{per}'";
                                break;
                            case ReportSortationPeriod.Yearly:
                                whereReportInject = $@"substr(""End date"", 7, 4) == '{per}'";
                                break;
                        }
                        if (!string.IsNullOrEmpty(whereFilterInject))
                        {
                            whereReportInject = whereReportInject.Insert(0, " AND ");
                        }
                        else
                        {
                            whereReportInject = whereReportInject.Insert(0, " WHERE ");
                        }

                        string commandString = "";
                        int recordCount = 0;
                        commandString = @$"SELECT COUNT(*) FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject}";
                        if (parameters.Count != 0)
                        {
                            recordCount = conn.ExecuteScalar<int>(commandString, parameters);
                        }
                        else
                        {
                            recordCount = conn.ExecuteScalar<int>(commandString);
                        }
                        if (recordCount == 0)
                        {
                            durationData.Remove(per);
                            linesData.Remove(per);
                            continue;
                        }

                        if (settings.DataOptions[0] == true)
                        {
                            if (settings.ReportOptions[0] == true)
                            {
                                if (parameters.Count != 0)
                                {
                                    durationData[per].Add(recordCount.ToString());
                                }
                                else
                                {
                                    durationData[per].Add(recordCount.ToString());
                                }
                            }

                            if (settings.ReportOptions[1] == true || settings.ReportOptions[2] == true || settings.ReportOptions[3] == true || settings.ReportOptions[4] == true || settings.ReportOptions[5] == true || settings.ReportOptions[6] == true)
                            {
                                commandString = @$"SELECT * FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject}";
                                System.Data.IDataReader reader;
                                if (parameters.Count != 0)
                                {
                                    DynamicParameters dynamicParameters = new DynamicParameters(parameters);
                                    reader = conn.ExecuteReader(commandString, dynamicParameters);
                                }
                                else
                                {
                                    reader = conn.ExecuteReader(commandString);
                                }

                                while (reader.Read())
                                {
                                    records.Add(new CodingSession(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6), reader.GetString(7), reader.GetInt32(8) == 1 ? true : false));
                                }
                                reader.Close();
                            }

                            if (settings.ReportOptions[1] == true)
                            {
                                TimeSpan totalDuration = TimeSpan.FromSeconds(0);
                                foreach (var record in records)
                                {
                                    totalDuration += TimeSpan.Parse(record.Duration);
                                }
                                durationData[per].Add(TimeSpanToPresentableString(totalDuration));
                            }
                            if (settings.ReportOptions[2] == true)
                            {
                                TimeSpan maxDuration = TimeSpan.MinValue;
                                foreach (var record in records)
                                {
                                    if (TimeSpan.Parse(record.Duration) > maxDuration)
                                    {
                                        maxDuration = TimeSpan.Parse(record.Duration);
                                    }
                                }
                                durationData[per].Add(TimeSpanToPresentableString(maxDuration));
                            }
                            if (settings.ReportOptions[3] == true)
                            {
                                TimeSpan minDuration = TimeSpan.MaxValue;
                                foreach (var record in records)
                                {
                                    if (TimeSpan.Parse(record.Duration) < minDuration)
                                    {
                                        minDuration = TimeSpan.Parse(record.Duration);
                                    }
                                }
                                durationData[per].Add(TimeSpanToPresentableString(minDuration));
                            }
                            if (settings.ReportOptions[4] == true)
                            {
                                TimeSpan totalDuration = TimeSpan.FromSeconds(0);
                                foreach (var record in records)
                                {
                                    totalDuration += TimeSpan.Parse(record.Duration);
                                }
                                totalDuration = totalDuration / records.Count;
                                durationData[per].Add(TimeSpanToPresentableString(totalDuration));
                            }
                            if (settings.ReportOptions[5] == true)
                            {
                                if (records.Count == 1)
                                {
                                    durationData[per].Add(TimeSpanToPresentableString(TimeSpan.Parse(records[0].Duration)));
                                }
                                else if (records.Count % 2 == 0)
                                {
                                    records = records.OrderByDescending(x => TimeSpan.Parse(x.Duration)).ToList();
                                    TimeSpan firstRecord = TimeSpan.Parse(records[(records.Count - 1) / 2].Duration);
                                    TimeSpan secondRecord = TimeSpan.Parse(records[(records.Count - 1) / 2 + 1].Duration);
                                    durationData[per].Add(TimeSpanToPresentableString((firstRecord + secondRecord) / 2));
                                }
                                else
                                {
                                    records = records.OrderBy(x => TimeSpan.Parse(x.Duration).Seconds).ToList();
                                    durationData[per].Add(TimeSpanToPresentableString(TimeSpan.Parse(records[(records.Count - 1) / 2].Duration)));
                                }
                            }
                            if (settings.ReportOptions[6] == true)
                            {
                                if (records.Count == 1)
                                {
                                    durationData[per].Add(TimeSpanToPresentableString(TimeSpan.Parse(records[0].Duration)));
                                }
                                else
                                {
                                    Dictionary<string, int> ocurrences = new Dictionary<string, int>();
                                    foreach (var record in records)
                                    {
                                        if (ocurrences.ContainsKey(record.Duration))
                                        {
                                            ocurrences[record.Duration] += 1;
                                        }
                                        else
                                        {
                                            ocurrences.Add(record.Duration, 1);
                                        }
                                    }
                                    ocurrences.OrderByDescending(x => x.Value);

                                    if (ocurrences.ElementAt(0).Value == ocurrences.ElementAt(1).Value)
                                    {
                                        durationData[per].Add("N/A");
                                    }
                                    else
                                    {
                                        durationData[per].Add(TimeSpanToPresentableString(TimeSpan.Parse(ocurrences.ElementAt(0).Key)));
                                    }
                                }
                            }
                        }
                        if (settings.DataOptions[1] == true)
                        {
                            commandString = @$"SELECT COUNT(*) FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
                            if (parameters.Count != 0)
                            {
                                recordCount = conn.ExecuteScalar<int>(commandString, parameters);
                            }
                            else
                            {
                                recordCount = conn.ExecuteScalar<int>(commandString);
                            }
                            if (recordCount == 0)
                            {
                                int reportOptionsCount = 0;
                                foreach (bool b in settings.ReportOptions)
                                {
                                    reportOptionsCount++;
                                }
                                for (int i = 0; i < linesData[per].Count; i++)
                                {
                                    linesData[per].Add("N/A");
                                }
                                continue;
                            }
                            if (settings.ReportOptions[0] == true)
                            {
                                commandString = @$"SELECT COUNT(*) FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
                                linesData[per].Add(recordCount.ToString());
                            }
                            if (settings.ReportOptions[1] == true)
                            {
                                commandString = @$"SELECT SUM(""Lines of code"") FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
                                if (parameters.Count != 0)
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString, parameters).ToString());
                                }
                                else
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString).ToString());
                                }
                            }
                            if (settings.ReportOptions[2] == true)
                            {
                                commandString = @$"SELECT MAX(""Lines of code"") FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
                                if (parameters.Count != 0)
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString, parameters).ToString());
                                }
                                else
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString).ToString());
                                }
                            }
                            if (settings.ReportOptions[3] == true)
                            {
                                commandString = @$"SELECT MIN(""Lines of code"") FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
                                if (parameters.Count != 0)
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString, parameters).ToString());
                                }
                                else
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString).ToString());
                                }
                            }
                            if (settings.ReportOptions[4] == true)
                            {
                                commandString = @$"SELECT AVG(""Lines of code"") FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
                                if (parameters.Count != 0)
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString, parameters).ToString());
                                }
                                else
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString).ToString());
                                }
                            }
                            if (settings.ReportOptions[5] == true)
                            {
                                int isOdd = recordCount % 2;
                                int isEven = isOdd == 1 ? 0 : 1;

                                if (recordCount == 1)
                                {
                                    commandString = @$"SELECT ""Lines of code"" FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";

                                    if (parameters.Count != 0)
                                    {
                                        System.Data.IDataReader reader = conn.ExecuteReader(commandString, parameters);
                                        reader.Read();
                                        linesData[per].Add(reader.GetString(0));
                                    }
                                    else
                                    {
                                        System.Data.IDataReader reader = conn.ExecuteReader(commandString);
                                        reader.Read();
                                        linesData[per].Add(reader.GetString(0));
                                    }
                                }
                                else
                                {
                                    commandString = @$"SELECT AVG(""Lines of code"") FROM (SELECT ""Lines of code"" FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1 ORDER BY ""Lines of code"" LIMIT 1 + {isEven} OFFSET {(recordCount - isOdd) / 2 - isEven})";
                                    if (parameters.Count != 0)
                                    {
                                        dynamic variable = conn.ExecuteScalar(commandString, parameters).ToString();
                                        linesData[per].Add(variable.ToString());
                                    }
                                    else
                                    {
                                        dynamic variable = conn.ExecuteScalar(commandString).ToString();
                                        linesData[per].Add(variable.ToString());
                                    }
                                }
                            }
                            if (settings.ReportOptions[6] == true)
                            {
                                if (recordCount == 1)
                                {
                                    commandString = @$"SELECT ""Lines of code"" FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";

                                    if (parameters.Count != 0)
                                    {
                                        System.Data.IDataReader reader = conn.ExecuteReader(commandString, parameters);
                                        reader.Read();
                                        linesData[per].Add(reader.GetString(0));
                                    }
                                    else
                                    {
                                        System.Data.IDataReader reader = conn.ExecuteReader(commandString);
                                        reader.Read();
                                        linesData[per].Add(reader.GetString(0));
                                    }
                                }
                                else
                                {
                                    System.Data.IDataReader reader;
                                    commandString = @$"SELECT ""Lines of code"", COUNT(*) FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1 GROUP BY ""Lines of code"" ORDER BY COUNT(*) DESC LIMIT 2";
                                    if (parameters.Count != 0)
                                    {
                                        reader = conn.ExecuteReader(commandString, parameters);
                                    }
                                    else
                                    {
                                        reader = conn.ExecuteReader(commandString);
                                    }
                                    Dictionary<string, string> modalDic = new Dictionary<string, string>();
                                    while (reader.Read())
                                    {
                                        modalDic.Add(reader.GetString(0), reader.GetString(1));
                                    }
                                    if (modalDic.ElementAt(0).Value == modalDic.ElementAt(1).Value)
                                    {
                                        linesData[per].Add("N/A");
                                    }
                                    else
                                    {
                                        linesData[per].Add(modalDic.ElementAt(0).Key);
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                DurationTable = durationData;
                LinesTable = linesData;
                return false;
            }
            DurationTable = null;
            LinesTable = null;
            return true;
        }
        public static List<Tuple<string, string>> ReturnAllWeeksOfTheYear(int year)
        {
            List<Tuple<string, string>> weeks = new();
            DateTime firstSundayOfYear = new DateTime(year, 1, 1);

            bool sundayCheck = true;
            while (sundayCheck)
            {
                if (firstSundayOfYear.DayOfWeek == DayOfWeek.Sunday)
                {
                    break;
                }
                firstSundayOfYear = firstSundayOfYear.AddDays(1);
            }
            weeks.Add(Tuple.Create("1." + new DateTime(year, 1, 1).ToString("yyyy-MM-dd"), firstSundayOfYear.ToString("yyyy-MM-dd")));

            DateTime previousSunday = firstSundayOfYear;
            DateTime thisSunday = firstSundayOfYear;

            int weekCount = 1;
            bool sameYearCheck = true;
            while (sameYearCheck)
            {
                weekCount++;
                previousSunday = thisSunday;
                thisSunday = thisSunday.AddDays(7);
                if (thisSunday.Year == year)
                {
                    weeks.Add(Tuple.Create(weekCount.ToString() + "." + previousSunday.AddDays(1).ToString("yyyy-MM-dd"), thisSunday.ToString("yyyy-MM-dd")));
                }
                else
                {
                    weeks.Add(Tuple.Create(weekCount.ToString() + "." + previousSunday.AddDays(1).ToString("yyyy-MM-dd"), new DateTime(year, 12, 31).ToString("yyyy-MM-dd")));
                    break;
                }
            }
            return weeks;
        }
        internal static List<string> ReturnAllRecordedYears()
        {
            using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
            {
                conn.Open();
                string comm = @$"SELECT DISTINCT substr(""End date"", 7, 4) FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} ORDER BY substr(""End date"", 7, 4)";
                System.Data.IDataReader reader = conn.ExecuteReader(comm);
                List<string> years = new List<string>();
                while (reader.Read())
                {
                    years.Add(reader.GetString(0));
                }
                conn.Close();
                return years;
            }
        }
        internal static List<int> ReturnRecordedMonthsForYear(int year)
        {
            List<int> monthsList = new List<int>();
            using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
            {
                conn.Open();
                string comm = @$"SELECT DISTINCT substr(""End date"", 4, 2) FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} WHERE CAST(substr(""End date"", 7, 4) AS INTEGER) = {year} ORDER BY substr(""End date"", 4, 2)";
                System.Data.IDataReader reader = conn.ExecuteReader(comm);
                while (reader.Read())
                {
                    monthsList.Add(reader.GetInt32(0));
                }
                conn.Close();
            }
            return monthsList;
        }
        internal static string TimeSpanToPresentableString(TimeSpan input)
        {
            string s = input.ToString();
            s = Regex.Replace(s, @"(?<=^1{1})\.{1}", " day, ");
            s = Regex.Replace(s, @"(?<=^[0-9]+)\.{1}", " days, ");
            s = Regex.Replace(s, @"([0-9]{2}):(?=[0-9]{2}:[0-9]+)", "$1h ");
            s = Regex.Replace(s, @"([0-9]{2}):(?=[0-9]+)", "$1m ");
            s = Regex.Replace(s, @"(?<=m )([0-9]{2})(.*)", "$1s");
            return s;
        }
    }
}
