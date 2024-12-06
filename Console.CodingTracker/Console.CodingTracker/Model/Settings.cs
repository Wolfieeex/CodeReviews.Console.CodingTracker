namespace Console.CodingTracker.Model;

internal class Settings
{
    internal static string ConnectionString
    {
        get; private set;
    } =
    "Data Source=C:\\Users\\aleks\\Desktop\\CSharpProjects\\CodeReviews.Console.CodingTracker\\Console.CodingTracker\\Databases\\Coding Tracker Database.db";

    internal static string DatabaseName { get; private set; } = "Tracking";
}