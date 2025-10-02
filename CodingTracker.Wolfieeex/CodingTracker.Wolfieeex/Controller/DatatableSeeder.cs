using static CodingTracker.Wolfieeex.Controller.MathHelpers;
using Microsoft.Extensions.Configuration;
using CodingTracker.Wolfieeex.Model;
using CodingTracker.Wolfieeex.Controller.DataHandlers;

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

        List<CodingSession> codingSessions = new List<CodingSession>();
        for (int i = 0; i < seederSettings.numOfLines; i++)
        {
            CodingSession session = new CodingSession();
            Random ran = new Random();
            /*string CreationDate = "";
            string LastUpdateDate = "";
            string StartDate = "";
            string EndDate = "";
            string Duration = "";
            int? NumberOfLines = null;
            string Comments = "";
            bool WasTimerTracked = false;*/

            long creationTick = RandomExponentialValueInRange(minYearTicks, maxYearTicks, 0.85);

            long duration = RandomExponentialValueInRange(seederSettings.minSessionTime, seederSettings.maxSessionTime, 1);
            TimeSpan timeSpanduration = new TimeSpan(0, 0, (int)duration);

            DateTime start = new DateTime(creationTick);
            StartDate = start.ToString();

            DateTime end = start.Add(timeSpanduration);
            DateTime update = end;

            DateTime creation = end;
            CreationDate = creation.ToString();

            // Decide, whether the session was updated by the user at the later time (the creation time and end time of the session will then differ).
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
                    bool sessionTimeDoesntFallBelowMinSessionTime = true;
                    while (sessionTimeDoesntFallBelowMinSessionTime)
                    {
                        int endTimeVariation = (int)RandomExponentialValueInRange((long)seederSettings.minEndTimeVariation, (long)seederSettings.maxEndTimeVariation, 0.4);
                        TimeSpan timeSpanEndTimeVariation = new TimeSpan(0, 0, endTimeVariation);
                        DateTime temptEnd = end;
                        temptEnd = end.Subtract(timeSpanEndTimeVariation);
                        if (temptEnd - start > new TimeSpan(0, 0, seederSettings.minSessionTime))
                        {
                            end = temptEnd;
                            sessionTimeDoesntFallBelowMinSessionTime = false;
                        }
                    }
                }
                update = end.Add(timeSpanUpdateVariation);
            }

            LastUpdateDate = update.ToString();
            EndDate = end.ToString();
            Duration = CalculateDuration(StartDate, EndDate).ToString();

            if (PercentageChanceGenerator(seederSettings.chanceThatLineWasUpdated))
                NumberOfLines = (int)RandomExponentialValueInRange(0, seederSettings.numOfLines, 0.9);

            if (PercentageChanceGenerator(seederSettings.chanceThatWasCommented))
            {
                int commentRoll = ran.Next(0, CodingSession.ProgrammingComments.Length);
                Comments = CodingSession.ProgrammingComments[commentRoll];
            }
            double TimerRoll = ran.NextDouble();
            WasTimerTracked = TimerRoll > seederSettings.chanceThatWasTimerTracked ? false : true;

            //CodingSession session = new CodingSession(CreationDate, LastUpdateDate, StartDate, EndDate, Duration, NumberOfLines!, Comments, WasTimerTracked.ToString());
            codingSessions.Add(session);
        }

        DataWriter dataWriter = new DataWriter();
        dataWriter.InjectMultipleRecords(codingSessions);
    }

    internal void CreateGoalMockTablebase()
    {
        int numOfGoalLines = 30;
        int maxGoalLengthInDays = 180;
        int maxGoalDistanceInThePast = 360;
        float chanceGoalInProgress = 0.8f;
        float chanceGoalFailedIfNotInProgress = 0.3f;
        float chanceForTaskBeingLines = 0.2f;

        Random random = new Random();
        UserGoal userGoal = new UserGoal();

        for (int i = 0; i < numOfGoalLines; i++)
        {
            if (PercentageChanceGenerator(chanceGoalInProgress))
            {
                userGoal.Status = "InProgress";
                userGoal.EndDate = "N/A";
            }
            else if (PercentageChanceGenerator(chanceGoalFailedIfNotInProgress))
            {
                userGoal.Status = "Failed";
            }
            else
            {
                userGoal.Status = "Completede";
            }

            if (PercentageChanceGenerator(chanceForTaskBeingLines))
                userGoal.GoalType = "Lines";
            else
                userGoal.GoalType = "Time";

            // Data for In Progress goals:
            TimeSpan goalTimeLength = new TimeSpan(random.Next(0, maxGoalLengthInDays + 1), 0, 0, 0);
            TimeSpan doneTimeSpan = TimeSpan.FromMilliseconds(RandomExponentialValueInRange(1, (long)goalTimeLength.TotalMilliseconds, 0.4f));
            TimeSpan leftTimeSpan = goalTimeLength - doneTimeSpan;
            TimeSpan inThePastAmount = new TimeSpan(random.Next(0, maxGoalDistanceInThePast + 1), 0, 0, 0);

           userGoal.StartDate = userGoal.Status switch
            {
                "InProgress" => (DateTime.Now - doneTimeSpan).ToString(),
                "Completed" or "Failed" => (DateTime.Now - inThePastAmount - goalTimeLength).ToString()
            };

            userGoal.EndDate = userGoal.Status switch
            {
                "InProgress" => (DateTime.Now + leftTimeSpan).ToString(),
                "Completed" or "Failed" => (DateTime.Now - inThePastAmount).ToString()
            };
        }

        for (int i = 0; i < 3 * numOfGoalLines; i++)
        {
            if (taskType == 0)
            {
                // Goal: Lines

                int totalGoal = random.Next(20, 101) * (goalTimeLength.Days < 1 ? 1 : goalTimeLength.Days);
                GoalAmount = totalGoal.ToString();
                GoalAmountLeft = MathF.Ceiling((float)(totalGoal * (leftTimeSpan1.TotalSeconds / goalTimeLength.TotalSeconds))).ToString();
            }
            else
            {
                // Goal: Time

                TimeSpan totalGoal = TimeSpan.FromDays(random.Next(1, 4) * (goalTimeLength.Days < 1 ? 1 : goalTimeLength.Days));
                GoalAmount = totalGoal.ToString(@"dd\.hh\:mm\:ss");
                GoalAmountLeft = TimeSpan.FromMilliseconds(RandomExponentialValueInRange(1, (long)leftTimeSpan1.TotalMilliseconds, 0.6f)).ToString();
            }
        }

        for (int i = 0; i < 2 * numOfGoalLines; i++)
        {
            if (taskType == 0)
            {
                // Goal: Lines

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
                // Goal: Time

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
        }
    }
}