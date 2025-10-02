using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using CodingTracker.Wolfieeex.Model;
using Spectre.Console;
using System.Text;
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
            seeder.CreateMainMockTablebase();
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

    internal void SetDefaultSettings(bool mainDbWasInitiated, bool goalDbWasInitiated)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.SetWindowSize(Console.LargestWindowWidth, System.Console.LargestWindowHeight);

        bool devOptionsForMockDatabasesEnabled = configuration.GetValue<bool>("DeveloperOptions:CreateMockDatabasesIfNone");

        if (mainDbWasInitiated && devOptionsForMockDatabasesEnabled)
            AnsiConsole.Markup("[yellow italic]A mock database was created and populated with random sessions inserted.[/]\n");
                
        if (goalDbWasInitiated && devOptionsForMockDatabasesEnabled)
            AnsiConsole.Markup("[yellow italic]A mock database was created and populated with random goals inserted.[/]\n");

        if ((mainDbWasInitiated || goalDbWasInitiated) && devOptionsForMockDatabasesEnabled)
        {
            AnsiConsole.Markup("\nThis was done based on a fact that you have dev options enabled." +
                " If you want to change this setting, go to \"appsettings.json\" file and change \"DeveloperOptions:CreateMockDatabasesIfNone\" to \"False\"." +
                "\n[yellow]Press any key to continue: [/]");
            System.Console.ReadKey();
        }
    }
}

