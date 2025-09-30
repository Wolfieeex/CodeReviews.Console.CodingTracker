using Microsoft.Extensions.Configuration;

namespace CodingTracker.Wolfieeex.Controller;

public abstract class DbConnectionProvider
{
	protected string connectionString { get; init; }
	protected string mainTableName { get; init; }
	protected string goalTableName { get; init; }
	protected bool createMockDatabases { get; init; }
	
	public DbConnectionProvider()
	{
		IConfiguration configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

		connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
		mainTableName = configuration.GetSection("DatabaseTableNames")["CodingTrackingDatabase"];
		goalTableName = configuration.GetSection("DatabaseTableNames")["GoalTrackingDatabase"];
		createMockDatabases = Convert.ToBoolean(configuration.GetSection("DeveloperOptions")["CreateMockDatabasesIfNone"]);
	}
}
