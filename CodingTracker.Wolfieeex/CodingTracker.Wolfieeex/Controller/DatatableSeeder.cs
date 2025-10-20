using static CodingTracker.Wolfieeex.Controller.MathHelpers;
using CodingTracker.Wolfieeex.Model;
using CodingTracker.Wolfieeex.Controller.DataHandlers;

namespace CodingTracker.Wolfieeex.Controller;

internal class DatatableSeeder
{
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

            long creationTick = RandomExponentialValueInRange(minYearTicks, maxYearTicks, 0.85);
            long duration = RandomExponentialValueInRange(seederSettings.minSessionTime, seederSettings.maxSessionTime, 1);
            TimeSpan timeSpanduration = new TimeSpan(0, 0, (int)duration);

            DateTime start = new DateTime(creationTick);
            session.StartDate = start.ToString();
            DateTime end = start.Add(timeSpanduration);
            DateTime update = end;
            DateTime creation = end;
            session.CreationDate = creation.ToString();

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
            session.LastUpdateDate = update.ToString();
            session.EndDate = end.ToString();
            session.Duration = CalculateDuration(session.StartDate, session.EndDate).ToString();

            if (PercentageChanceGenerator(seederSettings.chanceThatLineWasUpdated))
                session.LinesOfCode = (int)RandomExponentialValueInRange(0, seederSettings.numOfLines, 0.9);

            if (PercentageChanceGenerator(seederSettings.chanceThatWasCommented))
            {
                session.Comments = seederSettings.GenerateRandomComment();
            }
            double TimerRoll = ran.NextDouble();
            session.WasTimerTracked = TimerRoll > seederSettings.chanceThatWasTimerTracked ? "false" : "true";

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
        int maxGoalDistanceInThePastInDays = 360;
        float chanceGoalInProgress = 0.8f;
        float chanceGoalFailedIfNotInProgress = 0.3f;
        float chanceForTaskBeingLines = 0.2f;

        int minLinesGoal = 100;
        int maxLinesGoal = 50000;
        TimeSpan minTimeGoal = new TimeSpan(6, 0, 0);
        TimeSpan maxTimeGoal = new TimeSpan(18, 0, 0, 0);

        Random random = new Random();
        UserGoal userGoal = new UserGoal();

        for (int i = 0; i < numOfGoalLines; i++)
        {
            if (PercentageChanceGenerator(chanceGoalInProgress))
            {
                userGoal.Status = "InProgress";
                userGoal.FinishingDate = "N/A";
            }
            else if (PercentageChanceGenerator(chanceGoalFailedIfNotInProgress))
            {
                userGoal.Status = "Failed";
            }
            else
            {
                userGoal.Status = "Completed";
            }

            if (PercentageChanceGenerator(chanceForTaskBeingLines))
                userGoal.GoalType = "Lines";
            else
                userGoal.GoalType = "Time";

            // Data for In Progress goals:
            TimeSpan goalTimeLength = new TimeSpan(random.Next(0, maxGoalLengthInDays + 1), 0, 0, 0);
            TimeSpan doneTimeSpan = TimeSpan.FromMilliseconds(RandomExponentialValueInRange(1, (long)goalTimeLength.TotalMilliseconds, 0.4f));
            TimeSpan leftTimeSpan = goalTimeLength - doneTimeSpan;

            TimeSpan inThePastAmount = new TimeSpan(random.Next(0, maxGoalDistanceInThePastInDays + 1), 1, 0, 0);

            int linesGoal = random.Next(minLinesGoal, maxLinesGoal);
            TimeSpan timeGoal = TimeSpan.FromMinutes(random.Next((int)minTimeGoal.TotalMinutes, (int)maxTimeGoal.TotalMinutes));
            int remainingGoalLines = random.Next(0, linesGoal);
            TimeSpan remainingGoalTime = TimeSpan.FromMinutes(random.Next(1, (int)timeGoal.TotalMinutes));

			TimeSpan maxTimeOfCompletion = ((DateTime.Parse(userGoal.StartDate) + goalTimeLength) - (DateTime.Parse(userGoal.StartDate) + timeGoal));
			TimeSpan minTimeOfCompletion = maxTimeOfCompletion / 2;
            TimeSpan timeOfCompletion = TimeSpan.FromMinutes(random.Next((int)minTimeOfCompletion.TotalMinutes, (int)maxTimeOfCompletion.TotalMinutes));

			userGoal.StartDate = userGoal.Status switch
            {
                "InProgress" => (DateTime.Now - doneTimeSpan).ToString(),
                "Completed" => (DateTime.Now - inThePastAmount - goalTimeLength).ToString(),
                "Failed" => (DateTime.Now - inThePastAmount - goalTimeLength).ToString()
            };
            userGoal.DeadlineDate = userGoal.Status switch
            {
                "InProgress" => (DateTime.Now + leftTimeSpan).ToString(),
                "Completed" => (DateTime.Now - inThePastAmount).ToString(),
                "Failed" => (DateTime.Now - inThePastAmount).ToString()
            };
            userGoal.StartingGoal = (userGoal.GoalType, userGoal.Status) switch
            {
                ("Lines", "InProgress") => linesGoal.ToString(),
                ("Time", "InProgress") => timeGoal.ToString(),
				("Lines", "Completed") => linesGoal.ToString(),
				("Time", "Completed") => timeGoal.ToString(),
				("Lines", "Failed") => linesGoal.ToString(),
				("Time", "Failed") => timeGoal.ToString()
			};
            userGoal.RemainingGoal = (userGoal.GoalType, userGoal.Status) switch
            {
                ("Lines", "InProgress") => remainingGoalLines.ToString(),
				("Time", "InProgress") => remainingGoalTime.ToString(),
				("Lines", "Completed") => "Goal achiveved!",
                ("Time", "Completed") => "Goal achiveved!",
				("Lines", "Failed") => remainingGoalLines.ToString(),
				("Time", "Failed") => remainingGoalTime.ToString()
			};
            userGoal.FinishingDate = userGoal.Status switch
            {
                "InProgress" => "Still in progress",
                "Completed" => (DateTime.Parse(userGoal.StartDate) + timeGoal + timeOfCompletion).ToString(),
                "Failed" => (DateTime.Now - inThePastAmount).ToString()
            };
        }
    }
}