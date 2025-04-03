using Spectre.Console;
using System.Reflection;
using System.Configuration;
using System.Collections.Specialized;

namespace Console.CodingTracker.Model;

internal class Settings
{
    public static string ConnectionString
    {
        get
        {
            string conn = Assembly.GetEntryAssembly().Location;
            conn = conn.Remove(conn.LastIndexOf("bin") - 1);
            conn = conn.Remove(conn.LastIndexOf("\\") + 1) + "Databases\\Coding Tracker Database.db";
            conn = "Data Source=" + conn;
            return conn;
        }
    }
    internal static bool CreateMockTablebase { get; private set; } = true;
    internal static int MockTableBaseNumberOfLines { get; private set; } = 400;
    internal static int MockTableBaseMinYear { get; private set; } = 2023;
    internal static TimeSpan MockTableBaseMinTime { get; private set; } = new TimeSpan(0, 0, 45, 0);
    internal static TimeSpan MockTableBaseMaxTime { get; private set; } = new TimeSpan(1, 2, 15, 0);
    internal static TimeSpan MinUpdateTimeVariation { get; private set; } = new TimeSpan(2, 0, 0, 0);
    internal static TimeSpan MaxUpdateTimeVariation { get; private set; } = new TimeSpan(0, 0, 1, 0);
    internal static TimeSpan MinEndTimeVariation { get; private set; } = new TimeSpan(0, 2, 0, 0);
    internal static TimeSpan MaxEndTimeVariation { get; private set; } = new TimeSpan(0, 0, 15, 0);
    internal static string optionalsCompleted { get; private set; } = $"[#{Color.GreenYellow.ToHex()}]";
    internal static string optionalsNotCompleted { get; private set; } = $"[#{Color.Red3_1.ToHex()}]";
}