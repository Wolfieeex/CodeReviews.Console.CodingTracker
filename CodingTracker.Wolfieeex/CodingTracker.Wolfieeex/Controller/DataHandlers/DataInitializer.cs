using Microsoft.Data.Sqlite;
using CodingTracker.Wolfieeex.Model;
using Dapper;

namespace CodingTracker.Wolfieeex.Controller;

internal class DataInitializer : DbConnectionProvider
{
    private bool devOptionsSetUpMockDatabases;

    public DataInitializer() : base()
    {
        devOptionsSetUpMockDatabases = Convert.ToBoolean(configuration.GetSection("DeveloperOptions")["CreateMockDatabasesIfNone"]);
    }

    internal bool InstantiateMainDatabase()
    {
        bool createdMainDb = false;

        using SqliteConnection conn = new SqliteConnection(ConnectionString);
        conn.Open();
        string commandText = $@"CREATE TABLE IF NOT EXISTS '{mainTableName}' (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            CreationDate TEXT NOT NULL,
                            LastUpdateDate TEXT,
                            StartDate TEXT NOT NULL,
                            EndDate TEXT NOT NULL,
                            Duration TEXT NOT NULL,
                            LinesOfCode INT,
                            Comments TEXT,
                            WasTimerTracked TEXT NOT NULL
                            )";
        conn.Execute(commandText);

        commandText = $"SELECT * FROM '{mainTableName}'";
        List<CodingSession> codingSessions = conn.Query<CodingSession>(commandText).ToList();

        if (codingSessions.Count() == 0 && devOptionsSetUpMockDatabases)
        {
            DatatableSeeder seeder = new DatatableSeeder();
            seeder.CreateMockTablebase();
            createdMainDb = true;
        }
        return createdMainDb;
    }

    internal bool InstantiateGoalDatabase()
    {
        bool createdGoalMockDb = false;

        using SqliteConnection conn = new SqliteConnection(ConnectionString);
        conn.Open();
        string command = @$"CREATE TABLE IF NOT EXISTS {goalTableName} (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        GoalType TEXT,
                        Status TEXT,
                        StartDate TEXT,
                        EndDate TEXT,
                        StartingGoal TEXT,
                        RemainingGoal TEXT,
                        Deadline TEXT
                        )";
        conn.Execute(command);

        command = $"SELECT * FROM '{goalTableName}'";
        List<UserGoal> goals = conn.Query<UserGoal>(command).ToList();

        if (goals.Count() == 0 && devOptionsSetUpMockDatabases)
        {
            DatatableSeeder seeder = new DatatableSeeder();
            seeder.CreateGoalMockTablebase();
            createdGoalMockDb = true;
        }
        return createdGoalMockDb;
    }
}

