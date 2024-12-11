using Console.CodingTracker.Model;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Console.CodingTracker.Controller;

internal class SQLCommands
{
    public static List<Session> CurrentSessions { get; private set; } = new List<Session>();
    internal static string InjectRecord(string startDate, string endDate, int? lines, string comments)
    {
        TimeSpan codingSpan = CalculateDuration(startDate, endDate);
        string stringCodingSpan = codingSpan.ToString();

        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string commString = @$"INSERT INTO '{Settings.DatabaseName}' 
                                ('Creation date', 'Last update date', 'Start date', 'End date', Duration, 'Lines of code', Comments)
                                VALUES (@Creation, @Update, @Start, @End, @Duration, @Lines, @Comments)";
            var newRow = new { Creation = DateTime.Now, Update = DateTime.Now, Start = startDate, End = endDate, Duration = stringCodingSpan, Lines = lines.HasValue ? lines : -1, Comments = String.IsNullOrEmpty(comments) ? "" : comments };
            conn.Execute(commString, newRow);
            conn.Close();
        }
        return stringCodingSpan;
    }

    public static TimeSpan CalculateDuration(string s, string e)
    {
        return (DateTime.Parse(e) - DateTime.Parse(s));
    }

    internal static List<Session> GetRecords(FilterDetails filter)
    {
        List<Session> records = new List<Session>();

        string whereInject = "";
        if (filter != null)
        {
            if (!String.IsNullOrEmpty(filter.FromDate))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' >  substr('Start date', 7, 4) || '-' || substr('Start date', 4, 2) || '-' || substr('Start date', 1, 2) || ' ' || substr('Start date', 13, 5) || ':00 '";
            }
            if (!String.IsNullOrEmpty(filter.ToDate))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' >  substr('Start date', 7, 4) || '-' || substr('Start date', 4, 2) || '-' || substr('Start date', 1, 2) || ' ' || substr('Start date', 13, 5) || ':00 '";
            }
            if (!String.IsNullOrEmpty(filter.MinDuration))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' >  substr('Start date', 7, 4) || '-' || substr('Start date', 4, 2) || '-' || substr('Start date', 1, 2) || ' ' || substr('Start date', 13, 5) || ':00 '";
            }
            if (!String.IsNullOrEmpty(filter.MaxDuration))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' >  substr('Start date', 7, 4) || '-' || substr('Start date', 4, 2) || '-' || substr('Start date', 1, 2) || ' ' || substr('Start date', 13, 5) || ':00 '";
            }
            if (!String.IsNullOrEmpty(filter.MinLines))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' >  substr('Start date', 7, 4) || '-' || substr('Start date', 4, 2) || '-' || substr('Start date', 1, 2) || ' ' || substr('Start date', 13, 5) || ':00 '";
            }
            if (!String.IsNullOrEmpty(filter.MaxLines))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' >  substr('Start date', 7, 4) || '-' || substr('Start date', 4, 2) || '-' || substr('Start date', 1, 2) || ' ' || substr('Start date', 13, 5) || ':00 '";
            }
            if (!String.IsNullOrEmpty(filter.Comment))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' >  substr('Start date', 7, 4) || '-' || substr('Start date', 4, 2) || '-' || substr('Start date', 1, 2) || ' ' || substr('Start date', 13, 5) || ':00 '";
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

            System.Data.IDataReader reader = conn.ExecuteReader(commandString);
            while (reader.Read())
            {
                records.Add(new Session(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6), reader.GetString(7), false));
            }

            conn.Close();
        }

        CurrentSessions = records;

        return records;
    }

    internal static string DateTimeSqliteStringConvert(string datetime)
    {
        string returnDate = "";
        returnDate = datetime.Substring(6, 4) + "-" + datetime.Substring(3, 2) + "-" + datetime.Substring(0, 2) + " ";
        returnDate += datetime.Substring(12, 2) + ":" + datetime.Substring(15, 2) + ":" + "00";
        return returnDate;
    }
}
