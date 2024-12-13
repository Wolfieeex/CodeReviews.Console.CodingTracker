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
                                'Creation date' TEXT,
                                'Last update date' TEXT,
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

        long minYearTicks = new DateTime(minYear, 0, 0, 0, 0, 0).Ticks;
        long maxYearTicks = DateTime.Now.Subtract(Settings.MockTableBaseMaxTime).Ticks;

        for (int i = 0; i < Settings.MockTableBaseNumberOfLines; i++)
        {
            string CreationDate = "";
            string LastUpdateData = "";
            string StartDate = "";
            string EndDate = "";
            string Duration = "";
            int? NmberOfLines;
            string Comments = "";
            bool WasTimerTracked = false;

            int creationTick = RandomExponentialValueInRange((int)minYearTicks, (int)maxYearTicks, 0.35);
            
            int duration = RandomExponentialValueInRange((int)minSessionTime, (int)maxSessionTime, 0.15);
            TimeSpan timeSpanduration = new TimeSpan(0, 0, duration);

            DateTime start = new DateTime(creationTick);
            DateTime end = start.Add(timeSpanduration);
            DateTime creation = end;
            DateTime update;

            if (PercentageChanceGenerator(chanceThatWasUpdated))
            { 
                int updateVariation = RandomExponentialValueInRange((int)minUpdateVariation, (int)maxUpdateVariation, 0.1);
                TimeSpan timeSpanUpdateVariation = new TimeSpan(0, 0, updateVariation);

                if (PercentageChanceGenerator(0.5))
                {
                    int endTimeVariation = RandomExponentialValueInRange((int)minEndTimeVariation, (int)maxEndTimeVariation, 0.4);
                    TimeSpan timeSpanEndTimeVariation = new TimeSpan(0, 0, endTimeVariation);

                    end = end.Add(timeSpanEndTimeVariation);
                }
                else
                {
                    bool timeSpanIsNegative = true;
                    while (timeSpanIsNegative)
                    {
                        int endTimeVariation = RandomExponentialValueInRange((int)minEndTimeVariation, (int)maxEndTimeVariation, 0.4);
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

            //CreationDate = actual creation srring. Same for the rest of the dates, then duration. Rest should be easy peasy.




            string Creation = new DateTime().ToString();
        }
    }

    internal static int RandomExponentialValueInRange(int min, int max, double lambda)
    {
        if (lambda < 0 || lambda > 1)
        {
            throw new ArgumentException("Lambda value has to be in range of 0 and 1.");
        }

        Random ran = new Random();
        double roll = ran.NextDouble();

        return min + (max - min) * (int)Math.Round(Math.Pow(roll, lambda));
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
