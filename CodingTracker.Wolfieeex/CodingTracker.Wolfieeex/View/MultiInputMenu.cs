using Spectre.Console;

namespace CodingTracker.Wolfieeex.View;

internal abstract class MultiInputMenu : Menu
{
    public MultiInputMenu(Color color) : base(color) { }
    protected Dictionary<Enum, string> OptionKeys = new();
    protected virtual bool CheckInputConditions()
    {
        // Overall, get specialLabel of each enum. Do the same stuff with oneOf and required as before.
        // Additional checks can be included later on.
        return false;
    }
    protected override Enum DisplayOptions()
    {
        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }
    protected List<Enum> GenerateOptions()
    {
        // Check if confirm option can be generated (CheckInputConditions, then checking for the rest of required/oneOf prerequisits)
        // Generate options with their Dictionary Values
    }

    protected void AddKey()
    {

    }

    protected void RemoveKey()
    {
        
    }
}