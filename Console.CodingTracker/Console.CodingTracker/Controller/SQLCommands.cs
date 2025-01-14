using Console.CodingTracker.Model;
using Dapper;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Console.CodingTracker.Controller;

internal class SQLCommands
{
    public static List<Session> CurrentSessions { get; private set; } = new List<Session>();
    internal static string InjectRecord(Session session)
    {
        TimeSpan codingSpan = CalculateDuration(session.StartDate, session.EndDate);
        string stringCodingSpan = codingSpan.ToString();

        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string commString = @$"INSERT INTO '{Settings.DatabaseName}' 
                                ('Creation date', 'Last update date', 'Start date', 'End date', Duration, 'Lines of code', Comments, 'Was Timer Tracked')
                                VALUES (@Creation, @Update, @Start, @End, @Duration, @Lines, @Comments, @Timer)";
            var newRow = new { Creation = session.CreationDate, Update = session.LastUpdateData, Start = session.StartDate, End = session.EndDate, Duration = session.Duration, Lines = session.NumberOfLines.HasValue ? session.NumberOfLines : -1, Comments = String.IsNullOrEmpty(session.Comments) ? "" : session.Comments, Timer = session.WasTimerTracked ? 1 : 0 };
            conn.Execute(commString, newRow);
            conn.Close();
        }
        return stringCodingSpan;
    }

    public static TimeSpan CalculateDuration(string s, string e)
    {
        return (DateTime.Parse(e) - DateTime.Parse(s));
    }

    internal static List<Session> GetRecords(FilterDetails filter)
    {
        List<Session> records = new List<Session>();

        string whereInject = "";
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        if (filter != null)
        {
            
            if (!String.IsNullOrEmpty(filter.FromDate))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' <=  substr(""Start date"", 7, 4) || '-' || substr(""Start date"", 4, 2) || '-' || substr(""Start date"", 1, 2) || ' ' || substr(""Start date"", 13, 5) || ':00 '";
                parameters.Add("@FromDate", DateTimeSqliteStringConvert(filter.FromDate));
            }
            if (!String.IsNullOrEmpty(filter.ToDate))
            {
                whereInject += $@"AND '{DateTimeSqliteStringConvert(filter.FromDate)}' => substr(""Start date"", 7, 4) || '-' || substr(""Start date"", 4, 2) || '-' || substr(""Start date"", 1, 2) || ' ' || substr(""Start date"", 13, 5) || ':00 '";
            }
            if (!String.IsNullOrEmpty(filter.MinDuration))
            {
                whereInject += $@"AND ";
            }
            if (!String.IsNullOrEmpty(filter.MaxDuration))
            {
                whereInject += $@"AND ";
            }
            if (!String.IsNullOrEmpty(filter.MinLines))
            {
                whereInject += $@"AND ""Lines of code"" >= {filter.MinLines}";
            }
            if (!String.IsNullOrEmpty(filter.MaxLines))
            {
                whereInject += $@"AND ""Lines of code"" <= {filter.MaxLines}";
            }
            if (!String.IsNullOrEmpty(filter.Comment))
            {
                whereInject += $@"AND Comments LIKE '%{filter.Comment}%'";
            }

            if (whereInject.Contains("AND"))
            {
                whereInject = whereInject.Substring(4, whereInject.Length - 4);
                whereInject = whereInject.Insert(0, "WHERE ");
            }
        }

        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string commandString = @$"SELECT IIF(instr(Duration, '.') = 0, substr(Duration, 0, instr(Duration, '.') - 1), '8') || ' ' || substr(Duration, instr(Duration, ':') - 2, 2) || ' ' || substr(Duration, instr(Duration, ':') + 1, 2) || ' ' || substr(Duration, instr(Duration, ':') + 4, 2) FROM {Settings.DatabaseName}";

            //24 * (substr(Duration, 0, instr(Duration, '.') - 1) + substr(Duration, len(Duration) - 8, 2)
            // !! string commandString = @$"SELECT * FROM {Settings.DatabaseName} {whereInject}";
            System.Data.IDataReader reader;
            if (parameters.Count != 0)
            {
                DynamicParameters dynamicParameters = new DynamicParameters(parameters);
                reader = conn.ExecuteReader(commandString, dynamicParameters);
            }
            else
            {
                reader = conn.ExecuteReader(commandString);
            }

            while (reader.Read())
            {
                System.Console.WriteLine(reader.GetString(0));
                // !! records.Add(new Session(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6), reader.GetString(7), false));
            }

            conn.Close();
        }

        CurrentSessions = records;

        return records;
    }

    internal static string DateTimeSqliteStringConvert(string datetime)
    {
        string returnDate = "";
        returnDate = datetime.Substring(6, 4) + "-" + datetime.Substring(3, 2) + "-" + datetime.Substring(0, 2) + " ";
        returnDate += datetime.Substring(12, 2) + ":" + datetime.Substring(15, 2) + ":" + "00";
        return returnDate;
    }

    /*internal static string TimeSpanSqliteStringConvert(string timespan)
    {


        return;
    }*/
}
