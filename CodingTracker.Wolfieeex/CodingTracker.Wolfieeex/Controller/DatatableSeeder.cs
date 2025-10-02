using static CodingTracker.Wolfieeex.Controller.MathHelpers;
using Microsoft.Extensions.Configuration;
using CodingTracker.Wolfieeex.Model;

namespace CodingTracker.Wolfieeex.Controller;

internal class DatatableSeeder
{
    private IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

    internal void CreateMainMockTablebase()
    {
        SeederSettings seederSettings = new SeederSettings();

        long minYearTicks = new DateTime(seederSettings.minYear, 1, 1, 0, 0, 0).Ticks;
        long maxYearTicks = DateTime.Now.Subtract(TimeSpan.FromSeconds(seederSettings.maxSessionTime) + TimeSpan.FromSeconds(seederSettings.maxEndTimeVariation)).Ticks;

        for (int i = 0; i < seederSettings.numOfLines; i++)
        {
            Random ran = new Random();

            
            string LastUpdateDate = "";
            string StartDate = "";
            string EndDate = "";
            string Duration = "";
            int? NumberOfLines;
            string Comments = "";
            bool WasTimerTracked = false;

            long creationTick = RandomExponentialValueInRange(minYearTicks, maxYearTicks, 0.85);
            long duration = RandomExponentialValueInRange(seederSettings.minSessionTime, seederSettings.maxSessionTime, 1);
            TimeSpan timeSpanduration = new TimeSpan(0, 0, (int)duration);

            DateTime start = new DateTime(creationTick);
            DateTime end = start.Add(timeSpanduration);
            DateTime creation = end;
            DateTime update = end;

            if (PercentageChanceGenerator(seederSettings.chanceThatWasUpdated))
            {
                int updateVariation = (int)RandomExponentialValueInRange((long)seederSettings.minUpdateVariation, (long)seederSettings.maxUpdateVariation, 0.1);
                TimeSpan timeSpanUpdateVariation = new TimeSpan(0, 0, updateVariation);

                // End time will be later than the creation date of the record
                if (PercentageChanceGenerator(0.5))
                {
                    int endTimeVariation = (int)RandomExponentialValueInRange((long)seederSettings.minEndTimeVariation, (long)seederSettings.maxEndTimeVariation, 0.4);
                    TimeSpan timeSpanEndTimeVariation = new TimeSpan(0, 0, endTimeVariation);

                    end = end.Add(timeSpanEndTimeVariation);
                }
                // End time will be earlier than the creation date of the record
                else
                {
                    // Make sure, that after the update, session time is no lesser than the minimal session time
                    bool timeSpanIsNegative = true;
                    while (timeSpanIsNegative)
                    {
                        int endTimeVariation = (int)RandomExponentialValueInRange((long)seederSettings.minEndTimeVariation, (long)seederSettings.maxEndTimeVariation, 0.4);
                        TimeSpan timeSpanEndTimeVariation = new TimeSpan(0, 0, endTimeVariation);
                        DateTime temptEnd = end;
                        temptEnd = end.Subtract(timeSpanEndTimeVariation);
                        if (temptEnd - start > new TimeSpan(0, 0, seederSettings.minSessionTime))
                        {
                            end = temptEnd;
                            timeSpanIsNegative = false;
                        }
                    }

                }
                update = end.Add(timeSpanUpdateVariation);
            }

            string CreationDate = creation.ToString();
            LastUpdateDate = update.ToString();
            StartDate = start.ToString();
            EndDate = end.ToString();
            Duration = (end - start).ToString();

            if (PercentageChanceGenerator(seederSettings.chanceThatLineWasUpdated))
                NumberOfLines = (int)RandomExponentialValueInRange(0, seederSettings.numOfLines, 0.9);

            if (PercentageChanceGenerator(seederSettings.chanceThatWasCommented))
            {
                
                int commentRoll = ran.Next(0, CodingSession.ProgrammingComments.Length);
                Comments = CodingSession.ProgrammingComments[commentRoll];
            }
            double TimerRoll = ran.NextDouble();
            WasTimerTracked = TimerRoll > seederSettings.chanceThatWasTimerTracked ? false : true;

            CodingSession session = new CodingSession(CreationDate, LastUpdateDate, StartDate, EndDate, Duration, NumberOfLines, Comments, WasTimerTracked);
            Crud.InjectRecord(session);
        }
    }

    internal void CreateGoalMockTablebase()
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

            TimeSpan timeSpan1 = new TimeSpan(random.Next(0, 181), random.Next(0, 60), random.Next(0, 60), 0);
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

            using SqliteConnection conn = new SqliteConnection(Settings.ConnectionString)
            conn.Open();

            string command = @$"INSERT INTO {System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")}(Goal, Status, [Start Date], [End Date], [Start Goal Amount], [Goal Amount Left], [Finish Time])
                                VALUES ('{GoalType}', '{Status}', '{startDate}', '{deadline}', '{GoalAmount}', '{GoalAmountLeft}', '{FinishingTime}')";
            conn.Execute(command);
            
        }
    }
}