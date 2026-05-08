using CodingTracker.Wolfieeex.Model;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Data.SqlClient;

namespace CodingTracker.Wolfieeex.Controller;

internal class DataWriter : DbConnectionProvider
{
	internal string InjectCodingSession(CodingSession session)
	{
		using SqliteConnection conn = new SqliteConnection(connectionString);
		conn.Open();

		string commString = @$"INSERT INTO {mainTableName} 
							(CreationDate, LastUpdateDate, StartDate, EndDate, Duration, LinesOfCode, Comments, WasTimerTracked)
							VALUES (@CreationDate, @LastUpdateDate, @StartDate, @EndDate, @Duration, @LinesOfCode, @Comments, @WasTimerTracked)";
		conn.Execute(commString, session);
		return session.Duration;
	}

	internal void InjectMultipleCodingSessions(List<CodingSession> sessions)
	{
		using var conn = new SqliteConnection(connectionString);
		conn.Open();
		using SqliteTransaction transaction = conn.BeginTransaction();

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

	internal void InjectUserGoal(UserGoal goal)
	{
		using SqliteConnection connection = new SqliteConnection(connectionString);
		connection.Open();

		string sqlCommand = @$"INSERT INTO {goalTableName}
							(GoalType, Status, StartDate, FinishingDate, StartingGoal, RemainingGoal, DeadlineDate)
							VALUES (@GoalType, @Status, @StartDate, @FinishingDate, @StartingGoal, @RemainingGoal, @DeadlineDate)";
		connection.Execute(sqlCommand, goal);
	}
	
	internal void InjectMultipleUserGoals(List<UserGoal> goals)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
		SqliteTransaction transaction = connection.BeginTransaction();

		string sqlCommand = @$"INSERT INTO {goalTableName}
							(GoalType, Status, StartDate, FinishingDate, StartingGoal, RemainingGoal, DeadlineDate)
							VALUES (@GoalType, @Status, @StartDate, @FinishingDate, @StartingGoal, @RemainingGoal, @DeadlineDate)";

		try
		{
			connection.Execute(sqlCommand, goals, transaction: transaction);
			transaction.Commit();
		}
		catch(Exception ex)
        {
			transaction.Rollback();
			Console.WriteLine($"There was a problem while inserting multiple goals into the database: {ex.Message}");
        }
    }
}

