using Microsoft.Extensions.Configuration;

namespace CodingTracker.Wolfieeex.Controller;

internal abstract class DbConnectionProvider
{
	private string connectionString { get; init; }
	private string mainTable { get; init; }
	private string goalTable { get; init; }
	private bool createMockDatabases { get; init; }
	
	public DbConnectionProvider()
	{
		IConfiguration configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

		connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
		mainTable = configuration.GetSection("DatabaseTableNames")["CodingTrackingDatabase"];
		mainTable = configuration.GetSection("DatabaseTableNames")["GoalTrackingDatabase"];
		createMockDatabases = Convert.ToBoolean(configuration.GetSection("DeveloperOptions")["CreateMockDatabasesIfNone"]);
	}
}
