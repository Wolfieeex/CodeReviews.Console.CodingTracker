using Console.CodingTracker.Model;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Console.CodingTracker.Controller;

internal class SQLCommands
{
    public string path = "";
    internal static string InjectRecord(string startDate, string endDate, int? lines, string comments)
    {
        System.TimeSpan codingSpan = DateTime.Parse(endDate) - DateTime.Parse(startDate);
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

    internal static SqliteDataReader GetRecords(FilterDetails filter)
    {
            string whereInject = "";
            if (!String.IsNullOrEmpty(filter.FromDate))
            {
                whereInject += $@"AND ({filter.FromDate} <)";
            }



            using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
            {
                string commandString = @$"SELECT(*) 
                                        FROM {Settings.DatabaseName}
                                        WHERE ";
            }

        return null;
    }
}
