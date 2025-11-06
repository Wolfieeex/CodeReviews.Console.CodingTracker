using Spectre.Console;
using CodingTracker.Wolfieeex.Model;
using CodingTracker.Wolfieeex.Controller;

namespace CodingTracker.Wolfieeex.View;

internal class AddRecordMenuallyMenu : MultiInputMenu
{
    protected override string title => "Welcome to adding records menu. Here you can manually add a session to database, in case you forgot to track it using a timer. "
    + "Plesase make your selection: ";

    protected override Type selectionEnum => typeof(TrackNewSession);

    public AddRecordMenuallyMenu(Color color) : base(color) { }

    private CodingSession codingSession = new()
    {
        WasTimerTracked = "false"
    };

    public override void DisplayMenu()
    {

        bool menuLoop = true;
        while (menuLoop)
        {
            Enum userInput = DisplayOptions();

            switch (userInput)
            {
                case TrackNewSession.Confirm:
                    codingSession.CreationDate = codingSession.LastUpdateDate =  DateTime.Now.ToString("dd/MM/yy HH/mm");
                    codingSession.Duration = MathHelpers.CalculateDuration(codingSession.StartDate, codingSession.EndDate);
                    DataWriter dataWriter = new();
                    dataWriter.InjectRecord(codingSession);
                    break;
                case TrackNewSession.AddSessionStart:
                    codingSession.StartDate = AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Datetime, menuColors));
                    break;
                case TrackNewSession.AddSessionEnd:
                    codingSession.EndDate = AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Datetime, menuColors));
                    break;
                case TrackNewSession.AddSessionNumberOfLines:
                    codingSession.LinesOfCode = Int32.Parse(AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Number, menuColors)));
                    break;
                case TrackNewSession.AddSessionComments:
                    codingSession.Comments = AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Text, menuColors));
                    break;
                case TrackNewSession.ReturnToMainMenu:
                    return;
                default:
                    throw new ArgumentOutOfRangeException("Unkwon enum value detected in AddRecordManuallyMenu.");
            }
        }
    }

    protected override bool CheckInputConditions()
    {
        bool baseConditionsPassed = base.CheckInputConditions();
        bool dateValidityCondition = true;

        if (!String.IsNullOrEmpty(codingSession.StartDate) && !String.IsNullOrEmpty(codingSession.EndDate))
        {
            DateTime startDate = DateTime.Parse(codingSession.StartDate);
            DateTime endDate = DateTime.Parse(codingSession.EndDate);

            // Make sure that timeSpan is calculated, creation and lastly updated date matching the end date.
            if (startDate > endDate)
            {
                dateValidityCondition = false;
                reasonCodes.Add($"{menuColorsHex.titleWarningColor}Starting date of your session cannot be later than the end of it.[/]");
            }
        }
        return baseConditionsPassed && dateValidityCondition;
    }
}