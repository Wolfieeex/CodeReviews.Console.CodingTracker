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
                    DataWriter dataWriter = new();
                    dataWriter.InjectRecord(codingSession);
                    break;
                case TrackNewSession.AddSessionStart:
                    codingSession.StartDate = AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Datetime));
                    break;
                case TrackNewSession.AddSessionEnd:
                    codingSession.EndDate = AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Datetime));
                    break;
                case TrackNewSession.AddSessionNumberOfLines:
                    codingSession.LinesOfCode = Int32.Parse(AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Number)));
                    break;
                case TrackNewSession.AddSessionComments:
                    codingSession.Comments = AlterKey(userInput,
                    InputValidator.ValidateInput(ReadEnumDescription(userInput), ValidatorType.Text));
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

            // Only text validation is working now. Add for dateTime and number.
            // Correct it so the condition part (Attention) appears only once, when there is at least one reason.
            // Make sure that timeSpan is calculated, creation and lastly updated date matching the end date.
            if (startDate > endDate)
            {
                dateValidityCondition = false;
                reasonCodes.Add($"{menuColorsHex.titleWarningColor}Attention![/] To continue, you need to "
            + $"{menuColorsHex.titleHighlightColor}fill all of[/] required fields: "
            + $"{menuColorsHex.titleHighlightColor}{string.Join(", ", requiredFields)}.[/]");
            }
        }
        return baseConditionsPassed && dateValidityCondition;
    }
}