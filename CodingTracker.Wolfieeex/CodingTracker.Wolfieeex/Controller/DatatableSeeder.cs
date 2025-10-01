using Microsoft.Extensions.Configuration;

namespace CodingTracker.Wolfieeex.Controller;

internal class DatatableSeeder
{
    private IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

    internal void CreateMockTablebase()
    {
        int minYear = configuration.GetValue<int>("MockDatabaseOptions:BaseMinYear");
        int numOfLines = configuration.GetValue<int>("MockDatabaseOptions:BaseNumberOfLines");

        int minSessionTime = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:BaseMinTime").TotalSeconds;
        int maxSessionTime = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:BaseMaxTime").TotalSeconds;

        int minUpdateVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MinUpdateTimeVariation").TotalSeconds;
        int maxUpdateVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MaxUpdateTimeVariation").TotalSeconds;

        int minEndTimeVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MinEndTimeVariation").TotalSeconds;
        int maxEndTimeVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MaxEndTimeVariation").TotalSeconds;

        double chanceThatWasUpdated = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatWasUpdated");
        double chanceThatWasCommented = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatWasCommented");
        double chanceThatWasTimerTracked = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatWasTimerTracked");
        double chanceThatLineWasUpdated = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatLineWasUpdated");

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
            double ifCommentRoll = ran.NextDouble();
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

            using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
            {
                conn.Open();

                string command = @$"INSERT INTO {System.Configuration.ConfigurationManager.AppSettings.Get("GoalDatabaseName")}(Goal, Status, [Start Date], [End Date], [Start Goal Amount], [Goal Amount Left], [Finish Time])
                                  VALUES ('{GoalType}', '{Status}', '{startDate}', '{deadline}', '{GoalAmount}', '{GoalAmountLeft}', '{FinishingTime}')";

                conn.Execute(command);
            }
        }
    }
}