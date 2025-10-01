using Microsoft.Data.Sqlite;
using CodingTracker.Wolfieeex.Model;
using Dapper;

using System.Text;
using CodingTracker.Wolfieeex.Controller.SQL;
using Spectre.Console;

namespace CodingTracker.Wolfieeex.Controller;

internal class DataInitializer : DbConnectionProvider
{
    public DataInitializer() : base() { }
    internal bool InstantiateMainDatabase()
    {
        bool createdMainDb = false;
        bool createdGoalDb = false;

        using SqliteConnection conn = new SqliteConnection(Settings.ConnectionString);
        conn.Open();
        string commandText = $@"CREATE TABLE IF NOT EXISTS '{mainTableName}' (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            'Creation date' TEXT,
                            'Last update date' TEXT,
                            'Start date' TEXT,
                            'End date' TEXT,
                            Duration TEXT,
                            'Lines of code' INT,
                            Comments TEXT,
                            'Was Timer Tracked' TEXT
                            )";
        conn.Execute(commandText);

        commandText = $"SELECT * FROM '{mainTableName}'";
        System.Data.IDataReader reader = conn.ExecuteReader(commandText);

        bool isDbNull = false;
        if (!reader.Read() && bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DeveloperOptions")))
        {
            isDbNull = true;
        }

        conn.Close();

        if (Settings.CreateMockTablebase && isDbNull)
        {
            createdMainDb = true;
            CreateMockTablebase();
        }

        bool isDbNull = false;
        if (!reader.Read() && bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DeveloperOptions")))
        {
            isDbNull = true;
        }

        if (Settings.CreateMockTablebase && isDbNull)
        {
            createdGoalDb = true;
            CreateGoalMockTablebase();
        }

        return (createdMainDb, createdGoalDb);
    }

    internal bool InstantiateGoalDatabase()
    {
        using SqliteConnection conn = new SqliteConnection(Settings.ConnectionString);

        conn.Open();
        string commandText = @$"CREATE TABLE IF NOT EXISTS {System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")} (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Goal TEXT,
                                Status TEXT,
                                'Start Date' TEXT,
                                'End Date' TEXT,
                                'Start Goal Amount' TEXT,
                                'Goal Amount Left' TEXT,
                                'Finish Time' TEXT
                                )";
        conn.Execute(commandText);
        commandText = $"SELECT * FROM '{System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")}'";
        System.Data.IDataReader reader = connection.ExecuteReader(commandText);
    }
}

