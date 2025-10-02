namespace CodingTracker.Wolfieeex.Controller
{
	internal class DataWriter : DbConnectionProvider
	{
		internal string InjectRecord(CodingSession session)
		{
			TimeSpan codingSpan = Helpers.CalculateDuration(session.StartDate, session.EndDate);
			string stringCodingSpan = codingSpan.ToString();

			using SqliteConnection conn = new SqliteConnection(Settings.ConnectionString)
			conn.Open();

			string commString = @$"INSERT INTO {mainTableName} 
								(CreationDate, LastUpdateDate, StartDate, EndDate, Duration, LinesOfCode, Comments, WasTimerTracked)
								VALUES (@Creation, @Update, @Start, @End, @Duration, @Lines, @Comments, @Timer)";
			var newRow = new { Creation = session.CreationDate, Update = session.LastUpdateDate, Start = session.StartDate, End = session.EndDate, session.Duration, Lines = session.NumberOfLines.HasValue ? session.NumberOfLines : -1, Comments = string.IsNullOrEmpty(session.Comments) ? "" : session.Comments, Timer = session.WasTimerTracked ? 1 : 0 };
			conn.Execute(commString, newRow);
			conn.Close();
			
			return stringCodingSpan;
		}
	}
}
