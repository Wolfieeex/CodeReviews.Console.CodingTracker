using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using SQLitePCL;

namespace CodingTracker.Wolfieeex.Controller;

public abstract class DbConnectionProvider
{
	protected static string ConnectionString = "Data Source=CodingTracker.db";
	protected string connectionString { get; init; }
	protected string mainTableName { get; init; }
	protected string goalTableName { get; init; }
	protected bool createMockDatabases { get; init; }
	protected IConfiguration configuration { get; init; }
	
	public DbConnectionProvider()
	{
		try
		{
			configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

			string connectionString = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
			string mainTableName = configuration.GetValue<string>("DatabaseTableNames:CodingTracking");
			string goalTableName = configuration.GetValue<string>("DatabaseTableNames:GoalTracking");
			bool createMockDatabases = configuration.GetValue<bool>("DeveloperOptions:CreateMockDatabasesIfNone");
		}
		catch (Exception ex)
		{
			Console.WriteLine("There was a problem while loading DbConnectionProvider class: " + ex.Message);
		}
	}
}
