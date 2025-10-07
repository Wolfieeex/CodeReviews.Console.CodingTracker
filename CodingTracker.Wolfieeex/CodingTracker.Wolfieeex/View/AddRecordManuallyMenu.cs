using Spectre.Console;

namespace CodingTracker.Wolfieeex.View;

internal class AddRecordMenuallyMenu : MultiInputMenu
{
    protected override string title => "Welcome to adding menu. Here you can manually add a session to database, in case you forgot to track it using a timer. "
    + "Plesase make your selection: ";

    public AddRecordMenuallyMenu(Color color) : base(color) { }
    public override Enum DisplayMenu()
    {
        throw new NotImplementedException();
    }
}