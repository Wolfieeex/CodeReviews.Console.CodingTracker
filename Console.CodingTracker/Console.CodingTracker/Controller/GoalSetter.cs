using Console.CodingTracker.View;
using Console.CodingTracker.Model;
using Spectre.Console;

namespace Console.CodingTracker.Controller;

internal class GoalSetter
{
    private static Color titleColor = Color.Green3_1;
    private static Color mainColor = Color.SpringGreen2_1;  
    private static Color inputColor = Color.Chartreuse3;
    internal static void GoalSetterMenu()
    {
        string titleColorHex = "[#" + titleColor.ToHex() + "]";
        string mainColorHex = "[#" + mainColor.ToHex() + "]";
        string inputColorHex = "[#" + inputColor.ToHex() + "]";

        bool menuLoop = true;
        while (menuLoop)
        {
            int? userOption = UserInterface.DisplaySelectionUI($"You are currently in {titleColorHex}goal setting menu[/]. Please select one of the options below:", typeof(MenuSelections.GoalSetterMenu), mainColor);

            switch (userOption)
            {
                case 0:
                    SetNewGoal();
                    break;
                case 1:
                    ViewPreviousGoals();
                    break;
                case 2:
                    DeleteGoal();
                    break;
                case 3:
                    menuLoop = false;
                    break;
            }
        }
    }

    private static void SetNewGoal()
    {

    }
    private static void ViewPreviousGoals()
    {

    }
    private static void DeleteGoal()
    {
    }
}
