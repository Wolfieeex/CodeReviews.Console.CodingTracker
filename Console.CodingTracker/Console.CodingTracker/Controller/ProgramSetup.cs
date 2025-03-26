using Microsoft.Data.Sqlite;
using Console.CodingTracker.Model;
using Dapper;
using System.Text;
using Console.CodingTracker.Controller.SQL;
using System.Data.SqlClient;

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
                                'Creation date' TEXT,
                                'Last update date' TEXT,
                                'Start date' TEXT,
                                'End date' TEXT,
                                Duration TEXT,
                                'Lines of code' INT,
                                Comments TEXT,
                                'Was Timer Tracked' TEXT
                                )";
            new SqliteCommand(commString, conn).ExecuteNonQuery();

            commString = $"SELECT * FROM '{Settings.DatabaseName}'";
            System.Data.IDataReader reader = conn.ExecuteReader(commString);

            bool isDbNull = false;
            if (!reader.Read())
            {
                isDbNull = true;
            }

            conn.Close();

            if (Settings.CreateMockTablebase && isDbNull)
            {
                CreateMockTablebase();
            }
        }

        using (SqliteConnection connection = new SqliteConnection(Settings.ConnectionString))
        {
            connection.Open();
            string connComm = @$"CREATE TABLE IF NOT EXISTS {Settings.GoalDatabaseName} (
                                 Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 Goal TEXT,
                                 Status TEXT,
                                 'Goal amount left' TEXT,
                                 'Time left' TEXT
                                 )";
            new SqliteCommand(connComm, connection).ExecuteNonQuery();
            connection.Close();
        }
    }
    internal static void CreateMockTablebase()
    {
        int minYear = Settings.MockTableBaseMinYear;
        int numOfLines = Settings.MockTableBaseNumberOfLines;
        int minSessionTime = (int)Settings.MockTableBaseMinTime.TotalSeconds;
        int maxSessionTime = (int)Settings.MockTableBaseMaxTime.TotalSeconds;
        int minUpdateVariation = (int)Settings.MinUpdateTimeVariation.TotalSeconds;
        int maxUpdateVariation = (int)Settings.MaxUpdateTimeVariation.TotalSeconds;
        int minEndTimeVariation = (int)Settings.MinEndTimeVariation.TotalSeconds;
        int maxEndTimeVariation = (int)Settings.MaxEndTimeVariation.TotalSeconds;


        double chanceThatWasUpdated = 0.02;
        double chanceThatWasCommented = 0.9;
        double chanceThatWasTimerTracked = 0.95;
        double chanceThatLineWasUpdated = 0.98;

        long minYearTicks = new DateTime(minYear, 1, 1, 0, 0, 0).Ticks;
        long maxYearTicks = DateTime.Now.Subtract(Settings.MockTableBaseMaxTime).Ticks;

        for (int i = 0; i < Settings.MockTableBaseNumberOfLines; i++)
        {
            string CreationDate = "";
            string LastUpdateDate = "";
            string StartDate = "";
            string EndDate = "";
            string Duration = "";
            int? NumberOfLines;
            string Comments = "";
            bool WasTimerTracked = false;

            long creationTick = RandomExponentialValueInRange(minYearTicks, maxYearTicks, 0.85);
            
            long duration = RandomExponentialValueInRange(minSessionTime, maxSessionTime, 1);
            TimeSpan timeSpanduration = new TimeSpan(0, 0, (int)duration);

            DateTime start = new DateTime(creationTick);
            DateTime end = start.Add(timeSpanduration);
            DateTime creation = end;
            DateTime update;

            if (PercentageChanceGenerator(chanceThatWasUpdated))
            { 
                int updateVariation = (int)RandomExponentialValueInRange((long)minUpdateVariation, (long)maxUpdateVariation, 0.1);
                TimeSpan timeSpanUpdateVariation = new TimeSpan(0, 0, updateVariation);

                if (PercentageChanceGenerator(0.5))
                {
                    int endTimeVariation = (int)RandomExponentialValueInRange((long)minEndTimeVariation, (long)maxEndTimeVariation, 0.4);
                    TimeSpan timeSpanEndTimeVariation = new TimeSpan(0, 0, endTimeVariation);

                    end = end.Add(timeSpanEndTimeVariation);
                }
                else
                {
                    bool timeSpanIsNegative = true;
                    while (timeSpanIsNegative)
                    {
                        int endTimeVariation = (int)RandomExponentialValueInRange((long)minEndTimeVariation, (long)maxEndTimeVariation, 0.4);
                        TimeSpan timeSpanEndTimeVariation = new TimeSpan(0, 0, endTimeVariation);
                        DateTime temptEnd = end;

                        temptEnd = end.Subtract(timeSpanEndTimeVariation);
                        if (temptEnd - start > new TimeSpan(0, 0, minSessionTime))
                        {
                            end = temptEnd;
                            timeSpanIsNegative = false;
                        }
                    }
                    
                }
                update = end.Add(timeSpanUpdateVariation);
            }
            else
            {
                update = end;
            }

            CreationDate = creation.ToString($"dd/MM/yyyy, HH:MM");
            LastUpdateDate = update.ToString($"dd/MM/yyyy, HH:MM");
            StartDate = start.ToString($"dd/MM/yyyy, HH:MM");
            EndDate = end.ToString($"dd/MM/yyyy, HH:MM");
            Duration = (end - start).ToString();

            if (PercentageChanceGenerator(chanceThatLineWasUpdated))
            {
                NumberOfLines = (int)RandomExponentialValueInRange(0, (long)numOfLines, 0.9);
            }
            else
            {
                NumberOfLines = -1;
            }
                
            Random ran = new Random();
            double ifCommentRoll  = ran.NextDouble();
            if (ifCommentRoll < chanceThatWasCommented)
            {
                int commentRoll = ran.Next(0, CodingSession.ProgrammingComments.Length);
                Comments = CodingSession.ProgrammingComments[commentRoll];
            }
            double TimerRoll = ran.NextDouble();
            WasTimerTracked = TimerRoll > chanceThatWasTimerTracked ? false : true;

            CodingSession session = new CodingSession(CreationDate, LastUpdateDate, StartDate, EndDate, Duration, NumberOfLines, Comments, WasTimerTracked);
            
            Crud.InjectRecord(session);
        }
    }
    internal static void ConsoleSettings()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        System.Console.SetWindowSize(System.Console.LargestWindowWidth, System.Console.LargestWindowHeight);
    }
    internal static long RandomExponentialValueInRange(long min, long max, double lambda)
    {
        if (lambda < -1 || lambda > 1)
        {
            throw new ArgumentException("Lambda value has to be in range of -1 and 1.");
        }

        Random ran = new Random();
        double roll = ran.NextDouble();

        return min + (long)Math.Round((max - min) * Math.Pow(roll, lambda));
    }
    internal static bool PercentageChanceGenerator(double num)
    {
        if (num < 0 || num > 1)
        {
            throw new ArgumentException("This method's argument needs to range from 0 to 1.");
        }

        Random ran = new Random();
        double roll = ran.NextDouble();

        return num >= roll ? true : false;
    }
}
