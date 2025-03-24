using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;

namespace Console.CodingTracker.Controller.SQL;

public enum MenuGoalType
{
    ReturnToPreviousMenu,
    ToCommitCertainAmountOfTimeForProgramming,
    ToProduceCertainAmountOfCodingLines
}

internal enum GoalLimit
{
    BackToPreviousMenu,
    Custom,
    Days3,
    Days7,
    Week1,
    Weeks2,
    Month1,
    Months3,
    Months6,
    Year1,
}

internal static class GoalSettings
{
    
    private static Color titleColor = Color.LightGreen;
    private static Color mainColor = Color.Chartreuse1;
    private static Color inputColor = Color.Yellow4_1;

    private static string titleColorHex = $"[#{titleColor.ToHex()}]";
    private static string mainColorHex = $"[#{mainColor.ToHex()}]";
    private static string inputColorHex = $"[#{inputColor.ToHex()}]";

    internal static void GoalMenu()
    {
        bool runMenuLoop = true ;
        while (runMenuLoop)
        {
            string title = $"You are now in the {titleColorHex}goal menu[/]. Please select one of the options below: ";

            int? userInput = UserInterface.DisplaySelectionUI(title, typeof(MenuSelections.GoalSetterMenu), mainColor);

            switch (userInput)
            {
                case 0:
                    runMenuLoop = false ;
                    continue;
                case 1:
                    SetNewGoal();
                    break;
                case 2:
                    ViewPreviousGoals();
                    break;
                case 3:
                    DeleteGoals();
                    break;
            }
        }
    }

    private static void SetNewGoal()
    {
        bool runTypeSelectorMenu = true;
        while (runTypeSelectorMenu)
        {
            int? userInputType = UserInterface.DisplaySelectionUI($"Happy to see that {titleColorHex}you are ready to set your new goal![/]\nPlease select what is your goal: ", typeof(MenuGoalType), mainColor);
            if (userInputType! == 0)
            {
                break;
            }

            bool runTimeLimitMenu = true ;
            while (runTimeLimitMenu)
            {
                int? userInputTimeLimit = UserInterface.DisplaySelectionUI($"That's a good one! Please select {titleColorHex}the time limit for your goal[/]: ", typeof(GoalLimit), mainColor);

                if (userInputTimeLimit! == 0)
                {
                    runTypeSelectorMenu= false ;
                    break;
                }

                

                switch (userInputTimeLimit)
                {
                    case 1:
                        dynamic timeLimitCustom = UserInterface.DisplayTextUI($"Please select a custom time as your limit to complete your goal in the \"d hh:mm\" format.", TextUIOptions.TimeSpanOnly, titleColor, goalSetterTitle: true);
                        if (timeLimit)
                        string timeLimitString = timeLimitCustom.ToString();
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        break;
                }

                // Kolejny input, tym razem jesli chodzi o typ danych:

                switch (userInputType)
                {
                    case 1:
                        //dynamic r = UserInterface.DisplayTextUI("Please select ")
                        break;
                    case 2:
                        ViewPreviousGoals();
                        break;
                }
            }
            
        }
    }
    private static void ViewPreviousGoals()
    {

    }

    private static void DeleteGoals()
    {
        
    }
}
