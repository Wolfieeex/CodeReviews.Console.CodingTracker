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
                                ('Start date', 'End date', Duration, 'Lines of code', Comments)
                                VALUES (@Start, @End, @Duration, @Lines, @Comments)";
            var newRow = new { Start = startDate, End = endDate, Duration = stringCodingSpan, Lines = lines.HasValue ? lines : -1, Comments = String.IsNullOrEmpty(comments) ? "" : comments };
            conn.Execute(commString, newRow);
            conn.Close();
        }
        return stringCodingSpan;
    }
}
