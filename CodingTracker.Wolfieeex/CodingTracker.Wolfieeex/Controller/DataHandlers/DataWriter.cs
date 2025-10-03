using CodingTracker.Wolfieeex.Model;
using Microsoft.Data.Sqlite;
using Dapper;

namespace CodingTracker.Wolfieeex.Controller.DataHandlers;

internal class DataWriter : DbConnectionProvider
{
	internal string InjectRecord(CodingSession session)
	{
		using SqliteConnection conn = new SqliteConnection(ConnectionString);
		conn.Open();

		string commString = @$"INSERT INTO {mainTableName} 
							(CreationDate, LastUpdateDate, StartDate, EndDate, Duration, LinesOfCode, Comments, WasTimerTracked)
							VALUES (@Creation, @Update, @Start, @End, @Duration, @Lines, @Comments, @Timer)";
		conn.Execute(commString, session);
		return session.Duration;
	}

	internal void InjectMultipleRecords(List<CodingSession> sessions)
	{
		using var conn = new SqliteConnection(connectionString);
		conn.Open();
		using var transaction = conn.BeginTransaction();

		string commString = @$"INSERT INTO {mainTableName} 
			(CreationDate, LastUpdateDate, StartDate, EndDate, Duration, LinesOfCode, Comments, WasTimerTracked)
			VALUES (@CreationDate, @LastUpdateDate, @StartDate, @EndDate, @Duration, @LinesOfCode, @Comments, @WasTimerTracked)";
		try
		{
			conn.Execute(commString, sessions, transaction: transaction);
			transaction.Commit();
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine($"There was an error while multi-inserting sessions into the database: {ex.Message}");
		}
	}
}

