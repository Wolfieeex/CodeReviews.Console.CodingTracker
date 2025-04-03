using Console.CodingTracker.Model;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Configuration;

namespace Console.CodingTracker.Controller.SQL;

internal static class Helpers
{
    public static TimeSpan CalculateDuration(string s, string e)
    {
        return DateTime.Parse(e) - DateTime.Parse(s);
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
                string retreiveDatesCommand = @$"SELECT ""Start date"", ""End date"" FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} WHERE @id = Id";
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
    internal static List<string> GetEndDates(List<int> index)
    {
        List<string> dates = new();
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            for (int i = 0; i < index.Count; i++)
            {
                string commandString = $@"SELECT ""End date"" FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} WHERE @id = ID";
                dates.Add(conn.ExecuteScalar(commandString, new
                {
                    id = index[i]
                }).ToString());
            }
            conn.Close();
        }
        return dates;
    }
    internal static List<string> GetStartDates(List<int> index)
    {
        List<string> dates = new();
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            for (int i = 0; i < index.Count; i++)
            {
                string commandString = $@"SELECT ""Start date"" FROM {ConfigurationManager.AppSettings.Get("DatabaseName")} WHERE @id = ID";
                dates.Add(conn.ExecuteScalar(commandString, new
                {
                    id = index[i]
                }).ToString());
            }
            conn.Close();
        }
        return dates;
    }
    internal static string SqlDateToSortableDate(string date)
    {
        string returnDate = date.Substring(6, 4);
        returnDate += date.Substring(3, 2);
        returnDate += date.Substring(0, 2);
        returnDate += date.Substring(12, 5);
        return returnDate;
    }
}