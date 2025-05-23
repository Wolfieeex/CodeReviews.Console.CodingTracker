﻿using Microsoft.Data.Sqlite;
using Console.CodingTracker.Model;
using Dapper;
using System.Text;
using Console.CodingTracker.Controller.SQL;
using Spectre.Console;

namespace Console.CodingTracker.Controller;

internal class ProgramSetup
{
    internal static (bool, bool) InstantiateDatabase()
    {
		bool createdMainDb = false;
		bool createdGoalDb = false;

		using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();

            string dataBaseName = System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName");
			string commString = $@"CREATE TABLE IF NOT EXISTS '{dataBaseName}' (
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

            commString = $"SELECT * FROM '{System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName")}'";
            System.Data.IDataReader reader = conn.ExecuteReader(commString);

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
        }

        using (SqliteConnection connection = new SqliteConnection(Settings.ConnectionString))
        {
            connection.Open();
            string connComm = @$"CREATE TABLE IF NOT EXISTS {System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")} (
                                 Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 Goal TEXT,
                                 Status TEXT,
                                 'Start Date' TEXT,
                                 'End Date' TEXT,
                                 'Start Goal Amount' TEXT,
                                 'Goal Amount Left' TEXT,
                                 'Finish Time' TEXT
                                 )";
            new SqliteCommand(connComm, connection).ExecuteNonQuery();

			connComm = $"SELECT * FROM '{System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")}'";
			System.Data.IDataReader reader = connection.ExecuteReader(connComm);

			bool isDbNull = false;
			if (!reader.Read() && bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DeveloperOptions")))
			{
				isDbNull = true;
			}

			connection.Close();

			if (Settings.CreateMockTablebase && isDbNull)
			{
				createdGoalDb = true;
				CreateGoalMockTablebase();
			}
        }
        
        return (createdMainDb, createdGoalDb);
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
    internal static void CreateGoalMockTablebase()
    {
        int SettingsMultiplier = 10;

        string Status;
        string GoalType;
        string StartDate;
        string EndDate;
        string GoalAmount;
        string GoalAmountLeft;
        string FinishingTime;

        Random random = new Random();

        for (int i = 0; i < 3 * SettingsMultiplier; i++)
        {
            Status = "InProgress";
            FinishingTime = "N/A";

			TimeSpan timeSpan1 = new TimeSpan(random.Next(0,181), random.Next(0, 60), random.Next(0, 60), 0);
            TimeSpan doneTimeSpan1 = TimeSpan.FromMilliseconds(RandomExponentialValueInRange(1, (long)timeSpan1.TotalMilliseconds, 0.4f));
			TimeSpan leftTimeSpan1 = timeSpan1 - doneTimeSpan1;

            DateTime deadline = DateTime.Now + leftTimeSpan1;
            EndDate = deadline.ToString("dd/MM/yyyy HH:mm:ss");

			DateTime startDate = DateTime.Now - doneTimeSpan1;
			StartDate = startDate.ToString("dd/MM/yyyy HH:mm:ss");

            int taskType = random.Next(0, 2);

            if (taskType == 0)
            {
                GoalType = "Lines";

                int totalGoal = random.Next(20, 101) * (timeSpan1.Days < 1 ? 1 : timeSpan1.Days);
                GoalAmount = totalGoal.ToString();
                GoalAmountLeft = MathF.Ceiling((float)(totalGoal * (leftTimeSpan1.TotalSeconds / timeSpan1.TotalSeconds))).ToString();
            }
            else
            {
				GoalType = "Time";

				TimeSpan totalGoal = TimeSpan.FromDays(random.Next(1, 4) * (timeSpan1.Days < 1 ? 1 : timeSpan1.Days));
				GoalAmount = totalGoal.ToString(@"dd\.hh\:mm\:ss");
				GoalAmountLeft = TimeSpan.FromMilliseconds(RandomExponentialValueInRange(1, (long)leftTimeSpan1.TotalMilliseconds, 0.6f)).ToString();
			}

            using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
            {
                conn.Open();

                string command = @$"INSERT INTO {System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")}(Goal, Status, [Start Date], [End Date], [Start Goal Amount], [Goal Amount Left], [Finish Time])
                                  VALUES ('{GoalType}', '{Status}', '{startDate}', '{deadline}', '{GoalAmount}', '{GoalAmountLeft}', '{FinishingTime}')";

                conn.Execute(command);
            }
		}

		for (int i = 0; i < 2 * SettingsMultiplier; i++)
		{
			Status = random.Next(0, 2) == 0 ? "Failed" : "Completed";

			TimeSpan goalDuration = new TimeSpan(random.Next(0, 181), random.Next(0, 60), random.Next(0, 60), 0);
			TimeSpan doneTimeSpan1 = TimeSpan.FromMilliseconds(Math.Ceiling((double)RandomExponentialValueInRange(1, (long)goalDuration.TotalMilliseconds, 0.4f)));
			TimeSpan leftTimeSpan1 = goalDuration - doneTimeSpan1;

            TimeSpan inThePastAmount = new TimeSpan(random.Next(0, 61), random.Next(0, 60), random.Next(0, 60), 0);

            DateTime deadline = DateTime.Now - inThePastAmount;
			EndDate = deadline.ToString("dd/MM/yyyy HH:mm:ss");

            DateTime startDate = deadline - goalDuration;
			StartDate = startDate.ToString("dd/MM/yyyy HH:mm:ss");

			int taskType = random.Next(0, 2);

			if (taskType == 0)
			{
				GoalType = "Lines";

				int totalGoal = random.Next(10, 101) * (goalDuration.Days < 1 ? 1 : goalDuration.Days);
				GoalAmount = totalGoal.ToString();

                if (Status == "Failed")
                {
					FinishingTime = "DDL";
					GoalAmountLeft = MathF.Ceiling((float)(totalGoal * (leftTimeSpan1.TotalSeconds / goalDuration.TotalSeconds / 2))).ToString();
				}
				else
                {
                    FinishingTime = (startDate + doneTimeSpan1).ToString();
					GoalAmountLeft = "0";
				}
			}
			else
			{
				GoalType = "Time";

                TimeSpan totalGoal;

				if (Status == "Failed")
				{
					totalGoal = TimeSpan.FromSeconds(RandomExponentialValueInRange(2, (long)goalDuration.TotalSeconds, 0.5f));
					FinishingTime = "DDL";
					GoalAmountLeft = TimeSpan.FromSeconds(MathF.Ceiling(RandomExponentialValueInRange(1, (long)totalGoal.TotalSeconds, 0.6f))).ToString();
				}
				else
				{
					totalGoal = TimeSpan.FromDays(random.Next(1, 4) * (goalDuration.Days < 1 ? 1 : goalDuration.Days));
					GoalAmountLeft = "0";
					FinishingTime = (startDate + doneTimeSpan1).ToString();
				}

                GoalAmount = totalGoal.ToString(@"dd\.hh\:mm\:ss");
			}



			using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
			{
				conn.Open();

				string command = @$"INSERT INTO {System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")}(Goal, Status, [Start Date], [End Date], [Start Goal Amount], [Goal Amount Left], [Finish Time])
                                  VALUES ('{GoalType}', '{Status}', '{startDate}', '{deadline}', '{GoalAmount}', '{GoalAmountLeft}', '{FinishingTime}')";

				conn.Execute(command);
			}
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
	internal static void DisplayDevOptionSetting(bool main, bool goal)
	{
        if (main)
        {
			if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DeveloperOptions")))
			{
                AnsiConsole.Markup("[yellow italic]A mock database was created and populated with random sessions inserted.[/]\n");
			}
		}
        if (goal)
        {
			if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DeveloperOptions")))
			{
				AnsiConsole.Markup("[yellow italic]A mock database was created and populated with random goals inserted.[/]\n");
			}
		}
        if (main || goal)
		{
            AnsiConsole.Markup("\nThis was done based on a fact that you have dev options enabled." +
				" If you want to change this setting, go to \"App.config\" file and change \"DeveloperOptions\" to \"false\"." +
                "\n[yellow]Press any key to continue: [/]");
            System.Console.ReadKey();
		}
	}
}
