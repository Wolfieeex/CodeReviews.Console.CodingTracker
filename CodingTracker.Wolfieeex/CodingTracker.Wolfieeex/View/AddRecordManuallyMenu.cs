using Spectre.Console;
using CodingTracker.Wolfieeex.Model;

namespace CodingTracker.Wolfieeex.View;

internal class AddRecordMenuallyMenu : MultiInputMenu
{
    protected override string title => "Welcome to adding records menu. Here you can manually add a session to database, in case you forgot to track it using a timer. "
    + "Plesase make your selection: ";

    protected override Type selectionEnum => typeof(TrackNewSession);

    public AddRecordMenuallyMenu(Color color) : base(color) { }
    public override void DisplayMenu()
    {
        Enum userInput = DisplayOptions();

        switch (userInput)
        {
            case TrackNewSession.Confirm:

                break;
            case TrackNewSession.AddSessionStart:

                break;
            case TrackNewSession.AddSessionEnd:

                break;
            case TrackNewSession.AddSessionNumberOfLines:

                break;
            case TrackNewSession.AddSessionComments:

                break;
            case TrackNewSession.ReturnToMainMenu:

                break;
            default:
                throw new ArgumentOutOfRangeException("Unkwon enum value detected in AddRecordManuallyMenu.");
        }
    }
}