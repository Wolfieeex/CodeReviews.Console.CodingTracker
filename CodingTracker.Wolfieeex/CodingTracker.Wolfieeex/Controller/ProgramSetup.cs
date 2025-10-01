using Microsoft.Data.Sqlite;
using CodingTracker.Wolfieeex.Model;
using Dapper;
using System.Text;
using CodingTracker.Wolfieeex.Controller.SQL;
using Spectre.Console;

namespace CodingTracker.Wolfieeex.Controller;

internal class ProgramSetup
{
    internal static void ConsoleSettings()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        System.Console.SetWindowSize(System.Console.LargestWindowWidth, System.Console.LargestWindowHeight);
    }
    internal static long RandomExponentialValueInRange(long min, long max, double lambda)
    {
        if (lambda < -1 || lambda > 1)
        {
            throw new ArgumentException("Lambda value has to be in range of -1 and 1.");
        }

        Random ran = new Random();
        double roll = ran.NextDouble();

        return min + (long)Math.Round((max - min) * Math.Pow(roll, lambda));
    }
    internal static bool PercentageChanceGenerator(double num)
    {
        if (num < 0 || num > 1)
        {
            throw new ArgumentException("This method's argument needs to range from 0 to 1.");
        }

        Random ran = new Random();
        double roll = ran.NextDouble();

        return num >= roll ? true : false;
    }
	internal static void DisplayDevOptionSetting(bool main, bool goal)
	{
        if (main)
        {
			if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DeveloperOptions")))
			{
                AnsiConsole.Markup("[yellow italic]A mock database was created and populated with random sessions inserted.[/]\n");
			}
		}
        if (goal)
        {
			if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DeveloperOptions")))
			{
				AnsiConsole.Markup("[yellow italic]A mock database was created and populated with random goals inserted.[/]\n");
			}
		}
        if (main || goal)
		{
            AnsiConsole.Markup("\nThis was done based on a fact that you have dev options enabled." +
				" If you want to change this setting, go to \"appsettings.json\" file and change \"DeveloperOptions\" to \"False\"." +
                "\n[yellow]Press any key to continue: [/]");
            System.Console.ReadKey();
		}
	}
}
