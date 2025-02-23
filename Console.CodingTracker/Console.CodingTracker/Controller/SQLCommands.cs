using Console.CodingTracker.MenuSelections;
using Console.CodingTracker.Model;
using Dapper;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.Controller;

internal class SQLCommands
{
    public static List<CodingSession> CurrentSessions { get; private set; } = new List<CodingSession>();
    internal static string InjectRecord(CodingSession session)
    {
        TimeSpan codingSpan = CalculateDuration(session.StartDate, session.EndDate);
        string stringCodingSpan = codingSpan.ToString();

        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string commString = @$"INSERT INTO '{Settings.DatabaseName}' 
                                ('Creation date', 'Last update date', 'Start date', 'End date', Duration, 'Lines of code', Comments, 'Was Timer Tracked')
                                VALUES (@Creation, @Update, @Start, @End, @Duration, @Lines, @Comments, @Timer)";
            var newRow = new { Creation = session.CreationDate, Update = session.LastUpdateDate, Start = session.StartDate, End = session.EndDate, Duration = session.Duration, Lines = session.NumberOfLines.HasValue ? session.NumberOfLines : -1, Comments = String.IsNullOrEmpty(session.Comments) ? "" : session.Comments, Timer = session.WasTimerTracked ? 1 : 0 };
            conn.Execute(commString, newRow);
            conn.Close();
        }
        return stringCodingSpan;
    }
    internal static void UpdateRecords(List<int> indexNumbers, string newData, MenuSelections.UpdateMenu updateSegment)
    {
        string columnUpdateName = "";
        switch (updateSegment)
        {
            case MenuSelections.UpdateMenu.UpdateStartDate:
                columnUpdateName = "Start date";
                break;
            case MenuSelections.UpdateMenu.UpdateEndDate:
                columnUpdateName = "End date";
                break;
            case MenuSelections.UpdateMenu.UpdateNumberOfLines:
                columnUpdateName = "Lines of code";
                break;
            case MenuSelections.UpdateMenu.UpdateComments:
                columnUpdateName = "Comments";
                break;
        }
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string stringDateNow = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
            string updateDateCommand = $@"Update '{Settings.DatabaseName}' SET 'Last update date' = @stringDate WHERE Id = @id";
            string updateCommand = $@"Update '{Settings.DatabaseName}' SET '{columnUpdateName}' = @updateValue WHERE Id = @id";

            for (int i = 0; i < indexNumbers.Count; i++)
            {
                conn.Execute(updateDateCommand, new { stringDate = stringDateNow, id = indexNumbers[i] });
                conn.Execute(updateCommand, new { updateValue = newData, id = indexNumbers[i] });

                if (columnUpdateName == "Creation date" || columnUpdateName == "End date")
                {
                    string updateDurationCommand = $@"Update '{Settings.DatabaseName}' SET Duration = @duration WHERE Id = @id";
                    string retreiveDatesCommand = @$"SELECT ""Start date"", ""End date"" FROM {Settings.DatabaseName} WHERE Id = @id";
                    System.Data.IDataReader idr = conn.ExecuteReader(retreiveDatesCommand, new { id = indexNumbers[i] });
                    idr.Read();
                    string durationCalculated = CalculateDuration(idr.GetString(0), idr.GetString(1)).ToString();
                    conn.Execute(updateDurationCommand, new {duration = durationCalculated, id = indexNumbers[i] });
                    idr.Close();
                }
            }
            conn.Close();
        }
    }
    internal static List<string> GetStartDates(List<int> index)
    {
        List<string> dates = new();
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            for (int i = 0; i < index.Count; i++)
            {
                string commandString = $@"SELECT ""Start date"" FROM {Settings.DatabaseName} WHERE @id = ID";
                dates.Add(conn.ExecuteScalar(commandString, new
                {
                    id = index[i]
                }).ToString());
            }
            conn.Close();
        }
        return dates;
    }
    internal static List<string> GetEndDates(List<int> index)
    {
        List<string> dates = new();
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            for (int i = 0; i < index.Count; i++)
            {
                string commandString = $@"SELECT ""End date"" FROM {Settings.DatabaseName} WHERE @id = ID";
                dates.Add(conn.ExecuteScalar(commandString, new
                {
                    id = index[i]
                }).ToString());
            }
            conn.Close();
        }
        return dates;
    }
    internal static List<string> GetDurations(List<int> index)
    {
        List<string> durations = new();
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            System.Data.IDataReader idr;
            for (int i = 0; i < index.Count; i++)
            {
                string retreiveDatesCommand = @$"SELECT ""Start date"", ""End date"" FROM {Settings.DatabaseName} WHERE @id = Id";
                idr = conn.ExecuteReader(retreiveDatesCommand, new
                {
                    id = index[i]
                });
                idr.Read();
                durations.Add(CalculateDuration(idr.GetString(0), idr.GetString(1)).ToString());
            }
            conn.Close();
        }
        return durations;
    }
    internal static List<CodingSession> GetRecords(FilterDetails filter)
    {
        List<CodingSession> records = new List<CodingSession>();

        string whereInject = "";
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        whereInject = AddQueryFilterParameters(filter, whereInject, parameters);

        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();

            string commandString = @$"SELECT * FROM {Settings.DatabaseName} {whereInject}";
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

            int counter = 0;
            while (reader.Read())
            {
                records.Add(new CodingSession(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6), reader.GetString(7), reader.GetInt32(8) == 1 ? true : false));
            }

            conn.Close();
        }

        SortingDetails sortingDetails = filter.SortingDetails;

        if (sortingDetails != null)
        {
            MenuSelections.SortingOrder? sortingOrder = sortingDetails.SortOrder;
            MenuSelections.SortingBy? sortingBy = sortingDetails.SortBy;

            switch (sortingBy)
            {
                case MenuSelections.SortingBy.CreationDate:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => SqlDateToSortableDate(x.CreationDate)).ToList();
                    else
                        records = records.OrderByDescending(x => SqlDateToSortableDate(x.CreationDate)).ToList();
                    break;
                case MenuSelections.SortingBy.UpdateDate:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => SqlDateToSortableDate(x.LastUpdateDate)).ToList();
                    else
                        records = records.OrderByDescending(x => SqlDateToSortableDate(x.LastUpdateDate)).ToList();
                    break;
                case MenuSelections.SortingBy.StartDate:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => SqlDateToSortableDate(x.StartDate)).ToList();
                    else
                        records = records.OrderByDescending(x => SqlDateToSortableDate(x.StartDate)).ToList();
                    break;
                case MenuSelections.SortingBy.EndDate:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => SqlDateToSortableDate(x.EndDate)).ToList();
                    else
                        records = records.OrderByDescending(x => SqlDateToSortableDate(x.EndDate)).ToList();
                    break;
                case MenuSelections.SortingBy.Duration:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => x.Duration).ToList();
                    else
                        records = records.OrderByDescending(x => x.Duration).ToList();
                    break;
                case MenuSelections.SortingBy.NumberOfLines:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => x.NumberOfLines).ToList();
                    else
                        records = records.OrderByDescending(x => x.NumberOfLines).ToList();
                    break;
                case MenuSelections.SortingBy.Comment:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => x.Comments).ToList();
                    else
                        records = records.OrderByDescending(x => x.Comments).ToList();
                    break;
                case MenuSelections.SortingBy.WasTimerTracked:
                    if (sortingOrder == MenuSelections.SortingOrder.Ascending)
                        records = records.OrderBy(x => x.WasTimerTracked).ToList();
                    else
                        records = records.OrderByDescending(x => x.WasTimerTracked).ToList();
                    break;
            }
        }

        CurrentSessions = records;

        return records;
    }
    private static string AddQueryFilterParameters(FilterDetails filter, string whereInject, Dictionary<string, object> parameters)
    {
        if (filter != null)
        {

            if (!String.IsNullOrEmpty(filter.FromDate))
            {
                whereInject += $@"AND @FromDate <=  substr(""End date"", 7, 4) || '-' || substr(""End date"", 4, 2) || '-' || substr(""End date"", 1, 2) || ' ' || substr(""End date"", 13, 5) || ':00 ' ";
                parameters.Add("@FromDate", DateTimeSqliteStringConvert(filter.FromDate));
            }
            if (!String.IsNullOrEmpty(filter.ToDate))
            {
                whereInject += $@"AND @ToDate >= substr(""End date"", 7, 4) || '-' || substr(""End date"", 4, 2) || '-' || substr(""End date"", 1, 2) || ' ' || substr(""End date"", 13, 5) || ':00 ' ";
                parameters.Add("@ToDate", DateTimeSqliteStringConvert(filter.ToDate));
            }
            if (!String.IsNullOrEmpty(filter.MinDuration))
            {
                whereInject += $@"AND @MinDuration <= IIF(instr(Duration, '.') == 0, '', substr(Duration, 0, instr(Duration, '.'))) * 24 * 3600 + 3600 * substr(Duration, instr(Duration, ':') - 2, 2) + 60 * substr(Duration, instr(Duration, ':') + 1, 2) + substr(Duration, instr(Duration, ':') + 4, 2) ";
                parameters.Add("@MinDuration", TimeSpanSqliteStringConvert(filter.MinDuration));
            }
            if (!String.IsNullOrEmpty(filter.MaxDuration))
            {
                whereInject += $@"AND @MaxDuration >= IIF(instr(Duration, '.') == 0, '', substr(Duration, 0, instr(Duration, '.'))) * 24 * 3600 + 3600 * substr(Duration, instr(Duration, ':') - 2, 2) + 60 * substr(Duration, instr(Duration, ':') + 1, 2) + substr(Duration, instr(Duration, ':') + 4, 2) ";
                parameters.Add("@MaxDuration", TimeSpanSqliteStringConvert(filter.MaxDuration));
            }
            if (!String.IsNullOrEmpty(filter.MinLines))
            {
                whereInject += $@"AND ""Lines of code"" >= @MinLines ";
                parameters.Add("@MinLines", filter.MinLines);
            }
            if (!String.IsNullOrEmpty(filter.MaxLines))
            {
                whereInject += $@"AND ""Lines of code"" <= @MaxLines ";
                parameters.Add("@MaxLines", filter.MaxLines);
            }
            if (!String.IsNullOrEmpty(filter.Comment))
            {
                whereInject += $@"AND Comments LIKE lower(@Comment) ";
                parameters.Add("@Comment", "%" + filter.Comment.ToLower() + "%");
            }
            if (!String.IsNullOrEmpty(filter.WasTimerTracked))
            {
                whereInject += $@"AND ""Was Timer Tracked"" = @WasTimerTracked";
                parameters.Add("@WasTimerTracked", filter.WasTimerTracked == "True" ? 1 : 0);
            }

            if (whereInject.Contains("AND"))
            {
                whereInject = whereInject.Substring(4, whereInject.Length - 4);
                whereInject = whereInject.Insert(0, "WHERE ");
            }
        }

        return whereInject;
    }
    internal static void DeleteRecords(List<int> index)
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            foreach (int i in index) 
            {
                string command = @$"DELETE FROM '{Settings.DatabaseName}' WHERE @id = Id";
                conn.Execute(command, new { id = i });
            }
            conn.Close();
        }
    }
    internal static List<string> ReturnAllRecordedYears()
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string comm = @$"SELECT DISTINCT substr(""End date"", 7, 4) FROM {Settings.DatabaseName} ORDER BY substr(""End date"", 7, 4)";
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
            string comm = @$"SELECT DISTINCT substr(""End date"", 4, 2) FROM {Settings.DatabaseName} WHERE CAST(substr(""End date"", 7, 4) AS INTEGER) = {year} ORDER BY substr(""End date"", 4, 2)";
            System.Data.IDataReader reader = conn.ExecuteReader(comm);
            while (reader.Read())
            {
                monthsList.Add(reader.GetInt32(0));   
            }
            conn.Close();
        }
        return monthsList;
    }
    internal static string SqlDateToSortableDate(string date)
    {
        string returnDate = date.Substring(6, 4);
        returnDate += date.Substring(3, 2);
        returnDate += date.Substring(0, 2);
        returnDate += date.Substring(12, 5);
        return returnDate;
    }
    internal static string DateTimeSqliteStringConvert(string datetime)
    {
        string returnDate = "";
        returnDate = datetime.Substring(6, 4) + "-" + datetime.Substring(3, 2) + "-" + datetime.Substring(0, 2) + " ";
        returnDate += datetime.Substring(12, 2) + ":" + datetime.Substring(15, 2) + ":" + "00";
        return returnDate;
    }
    internal static int TimeSpanSqliteStringConvert(string timespan)
    {
        int timespanCalculated = 0;
        int spacePosition = timespan.IndexOf(' ');
        int ColonPosition = timespan.IndexOf(':');
        timespanCalculated += spacePosition == -1 ? 0 : Int32.Parse(timespan.Substring(0, spacePosition)) * 24 * 3600;
        timespanCalculated += Int32.Parse(timespan.Substring(ColonPosition - 2, 2)) * 3600;
        timespanCalculated += Int32.Parse(timespan.Substring(ColonPosition + 1, 2)) * 60;

        return timespanCalculated;
    }
    public static TimeSpan CalculateDuration(string s, string e)
    {
        return (DateTime.Parse(e) - DateTime.Parse(s));
    }
    public static void CalculateReport(ReportSettings settings, out Dictionary<string, List<string>> DurationTable, out Dictionary<string, List<string>> LinesTable)
    {
        List<CodingSession> sessions = GetRecords(settings.FilterDetails);
        if (sessions.Count == 0)
        {
            System.Console.Clear();
            AnsiConsole.WriteLine("Cannot generate report with selected settings- no records found. [red]Please select different settings and try again.[/]");
        }
        else
        {
            Dictionary<string, List<string>> durationData = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> linesData = new Dictionary<string, List<string>>();

            string whereFilterInject = "";
            string whereReportInject = "";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            whereFilterInject = AddQueryFilterParameters(settings.FilterDetails, whereFilterInject, parameters);
            
            List<string> periods = new List<string>();
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
                    // Not working!
                    periods = ReturnRecordedMonthsForYear(settings.SortationYear != null ? settings.SortationYear.Value : 0).Select(x => x.ToString("00")).ToList();
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

                foreach (string per in periods)
                {
                    durationData.Add(per, new List<string>());
                    linesData.Add(per, new List<string>());
                    switch (settings.Period)
                    {
                        case ReportSortationPeriod.Daily:
                            whereReportInject = $@"substr(""End date"", 1, 2) == '{per}'";
                            break;
                        case ReportSortationPeriod.Weekly:
                            whereReportInject = $@"substr(""End date"", 4, 2) == '{per}'";
                            break;
                        case ReportSortationPeriod.Monthly:
                            whereReportInject = $@"substr(""End date"", 4, 2) == '{per}'";
                            break;
                        case ReportSortationPeriod.Yearly:
                            whereReportInject = $@"substr(""End date"", 7, 4) == '{per}'";
                            break;
                    }
                    if (!String.IsNullOrEmpty(whereFilterInject))
                    {
                        whereReportInject = whereReportInject.Insert(0, " AND ");
                    }
                    else
                    {
                        whereReportInject = whereReportInject.Insert(0, " WHERE ");
                    }

                    string commandString = "";
                    int recordCount = 0;
                    commandString = @$"SELECT COUNT(*) FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject}";
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

                        List<CodingSession> records = new List<CodingSession>();
                        if (settings.ReportOptions[1] == true || settings.ReportOptions[2] == true || settings.ReportOptions[3] == true || settings.ReportOptions[4] == true || settings.ReportOptions[5] == true || settings.ReportOptions[6] == true)
                        {
                            commandString = @$"SELECT * FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject}";
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
                            durationData[per].Add(TimeSpanToPresentableString((totalDuration)));
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
                            durationData[per].Add(TimeSpanToPresentableString((maxDuration)));
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
                            durationData[per].Add(TimeSpanToPresentableString((minDuration)));
                        }
                        if (settings.ReportOptions[4] == true)
                        {   
                            TimeSpan totalDuration = TimeSpan.FromSeconds(0);
                            foreach (var record in records)
                            {
                                totalDuration += TimeSpan.Parse(record.Duration);
                            }
                            totalDuration = totalDuration / records.Count;
                            durationData[per].Add(TimeSpanToPresentableString((totalDuration)));
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
                                TimeSpan secondRecord = TimeSpan.Parse(records[((records.Count - 1) / 2) + 1].Duration);
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
                        commandString = @$"SELECT COUNT(*) FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
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
                            commandString = @$"SELECT COUNT(*) FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
                            linesData[per].Add(recordCount.ToString());
                        }
                        if (settings.ReportOptions[1] == true)
                        {
                            commandString = @$"SELECT SUM(""Lines of code"") FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
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
                            commandString = @$"SELECT MAX(""Lines of code"") FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
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
                            commandString = @$"SELECT MIN(""Lines of code"") FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
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
                            commandString = @$"SELECT AVG(""Lines of code"") FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1";
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
                                commandString = @$"SELECT ""Lines of code"" FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1 AND ROWNUM = 1";
                                if (parameters.Count != 0)
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString, parameters).ToString());
                                }
                                else
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString).ToString());
                                }
                            }
                            else
                            {
                                commandString = @$"SELECT AVG(""Lines of code"") FROM (SELECT ""Lines of code"" FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1 ORDER BY ""Lines of code"" LIMIT 1 + {isEven} OFFSET {((recordCount - isOdd) / 2) - isEven})";
                                if (parameters.Count != 0)
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString, parameters).ToString());
                                }
                                else
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString).ToString());
                                }
                            }
                        }
                        if (settings.ReportOptions[6] == true)
                        {
                            if (recordCount == 1)
                            {
                                commandString = @$"SELECT ""Lines of code"" FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1 AND ROWNUM = 1";
                                if (parameters.Count != 0)
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString, parameters).ToString());
                                }
                                else
                                {
                                    linesData[per].Add(conn.ExecuteScalar(commandString).ToString());
                                }
                            }
                            else
                            {
                                System.Data.IDataReader reader;
                                commandString = @$"SELECT ""Lines of code"", COUNT(*) FROM {Settings.DatabaseName} {whereFilterInject}{whereReportInject} AND ""Lines of code"" <> -1 GROUP BY ""Lines of code"" ORDER BY COUNT(*) DESC LIMIT 2";
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
            return;
        }
        DurationTable = null;
        LinesTable = null;
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
