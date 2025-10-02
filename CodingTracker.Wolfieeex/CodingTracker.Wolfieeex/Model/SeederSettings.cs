using Microsoft.Extensions.Configuration;

namespace CodingTracker.Wolfieeex.Model;

internal class SeederSettings
{
    private IConfiguration configuration { get; init; }
    public int minYear { get; set; }
    public int numOfLines { get; set; }
    public int minSessionTime { get; set; }
    public int maxSessionTime { get; set; }
    public int minUpdateVariation { get; set; }
    public int maxUpdateVariation { get; set; }
    public int minEndTimeVariation { get; set; }
    public int maxEndTimeVariation { get; set; }
    public double chanceThatWasUpdated { get; set; }
    public double chanceThatWasCommented { get; set; }
    public double chanceThatWasTimerTracked { get; set; }
    public double chanceThatLineWasUpdated { get; set; }

    public SeederSettings()
    {
        configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        minYear = configuration.GetValue<int>("MockDatabaseOptions:BaseMinYear");
        numOfLines = configuration.GetValue<int>("MockDatabaseOptions:BaseNumberOfLines");

        minSessionTime = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:BaseMinTime").TotalSeconds;
        maxSessionTime = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:BaseMaxTime").TotalSeconds;

        minUpdateVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MinUpdateTimeVariation").TotalSeconds;
        maxUpdateVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MaxUpdateTimeVariation").TotalSeconds;

        minEndTimeVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MinEndTimeVariation").TotalSeconds;
        maxEndTimeVariation = (int)configuration.GetValue<TimeSpan>("MockDatabaseOptions:MaxEndTimeVariation").TotalSeconds;

        chanceThatWasUpdated = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatWasUpdated");
        chanceThatWasCommented = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatWasCommented");
        chanceThatWasTimerTracked = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatWasTimerTracked");
        chanceThatLineWasUpdated = configuration.GetValue<double>("MockDatabaseOptions:ChanceThatLineWasUpdated");
    }
}