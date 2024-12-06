using Microsoft.Data.Sqlite;
using Console.CodingTracker.Model;

namespace Console.CodingTracker.Controller;

internal class ProgramSetup
{
    internal static void InstantiateDatabase()
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();

            string commString = $@"CREATE TABLE IF NOT EXISTS '{Settings.DatabaseName}' (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                'Start date' TEXT,
                                'End date' TEXT,
                                Duration TEXT,
                                'Lines of code' INT,
                                Comments TEXT
                                )";
            new SqliteCommand(commString, conn).ExecuteNonQuery();

            conn.Close();
        }
    }
}
