﻿using Console.CodingTracker.Model;
using Dapper;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;

namespace Console.CodingTracker.Controller;

internal class SQLCommands
{
    public static List<Session> CurrentSessions { get; private set; } = new List<Session>();
    internal static string InjectRecord(Session session)
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
                columnUpdateName = "Creation date";
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
            string stringDateNow = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
            string updateDateCommand = $@"Update '{Settings.DatabaseName}' SET 'Last update date' = @stringDate WHERE Id = @id";
            string updateDurationCommand = $@"Update '{Settings.DatabaseName}' SET Duration = @duration WHERE Id = @id";
            string updateCommand = $@"Update '{Settings.DatabaseName}' SET '{columnUpdateName}' = @updateValue WHERE Id = @id";

            // Update duration segment
            string duration = CalculateDuration()

            for (int i = 0; i < indexNumbers.Count; i++)
            {
                conn.Execute(updateDateCommand, new { stringDate = stringDateNow, id = indexNumbers[i] });
                //conn.Execute(updateDurationCommand, new { duration = stringDateNow, id = indexNumbers[i] });
                conn.Execute(updateCommand, new { updateValue = newData, id = indexNumbers[i] });
            }
        }
    }
    internal static List<Session> GetRecords(FilterDetails filter)
    {
        List<Session> records = new List<Session>();

        string whereInject = "";
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        if (filter != null)
        {
            
            if (!String.IsNullOrEmpty(filter.FromDate))
            {
                whereInject += $@"AND @FromDate <=  substr(""Start date"", 7, 4) || '-' || substr(""Start date"", 4, 2) || '-' || substr(""Start date"", 1, 2) || ' ' || substr(""Start date"", 13, 5) || ':00 ' ";
                parameters.Add("@FromDate", DateTimeSqliteStringConvert(filter.FromDate));
            }
            if (!String.IsNullOrEmpty(filter.ToDate))
            {
                whereInject += $@"AND @ToDate => substr(""End date"", 7, 4) || '-' || substr(""End date"", 4, 2) || '-' || substr(""End date"", 1, 2) || ' ' || substr(""End date"", 13, 5) || ':00 ' ";
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
                whereInject += $@"AND Comments LIKE '%@Comment%' ";
                parameters.Add("@Comment", filter.Comment);
            }

            if (whereInject.Contains("AND"))
            {
                whereInject = whereInject.Substring(4, whereInject.Length - 4);
                whereInject = whereInject.Insert(0, "WHERE ");
            }
        }

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
                records.Add(new Session(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6), reader.GetString(7), reader.GetInt32(8) == 1 ? true : false));
            }

            conn.Close();
        }

        SortingDetails sortingDetails = filter.sortingDetails;

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
                case MenuSelections.SortingBy.Line:
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
            }
        }

        CurrentSessions = records;

        return records;
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
    internal static string TimeSpanSqliteStringConvert(string timespan)
    {
        int timespanCalculated = 0;
        int spacePosition = timespan.IndexOf(' ');
        int ColonPosition = timespan.IndexOf(':');
        timespanCalculated += spacePosition == -1 ? 0 : Int32.Parse(timespan.Substring(0, spacePosition)) * 24 * 3600;
        timespanCalculated += Int32.Parse(timespan.Substring(ColonPosition - 2, 2)) * 3600;
        timespanCalculated += Int32.Parse(timespan.Substring(ColonPosition + 1, 2)) * 60;

        return timespanCalculated.ToString();
    }
    public static TimeSpan CalculateDuration(string s, string e)
    {
        return (DateTime.Parse(e) - DateTime.Parse(s));
    }
}
