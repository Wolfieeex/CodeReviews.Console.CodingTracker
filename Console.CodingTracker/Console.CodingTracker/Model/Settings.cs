namespace Console.CodingTracker.Model;

internal class Settings
{
    internal static string ConnectionString
    {
        get; private set;
    } =
    "Data Source=C:\\Users\\aleks\\Desktop\\CSharpProjects\\CodeReviews.Console.CodingTracker\\Console.CodingTracker\\Databases\\Coding Tracker Database.db";

    internal static string DatabaseName { get; private set; } = "Trackingv2";
    internal static bool CreateMockTablebase { get; private set; } = true;
    internal static int MockTableBaseNumberOfLines { get; private set; } = 400;
    internal static int MockTableBaseMinYear { get; private set; } = 2023;
    internal static TimeSpan MockTableBaseMinTime { get; private set; } = new TimeSpan(0, 0, 45, 0);
    internal static TimeSpan MockTableBaseMaxTime { get; private set; } = new TimeSpan(1, 2, 15, 0);
}